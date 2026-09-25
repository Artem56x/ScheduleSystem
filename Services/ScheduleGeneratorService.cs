using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using ScheduleSystem.Models.Generator;

namespace ScheduleSystem.Services;

public class ScheduleGeneratorService
{
    private readonly ApplicationDbContext _context;

    private const int MaxSearchNodes = 100_000;

    public ScheduleGeneratorService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    public async Task<ScheduleGenerationResult> GenerateAsync(
        ScheduleGenerationRequest request)
    {
        var result = new ScheduleGenerationResult();

        // --------------------------------------------------------
        // BASIC REQUEST VALIDATION
        // --------------------------------------------------------

        var validationErrors = ValidateRequest(request);

        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                result.AddError(error);
            }

            return result;
        }

        var selectedGroupIds = request.SelectedGroupIds
            .Distinct()
            .ToHashSet();

        var selectedDays = request.Days
            .Distinct()
            .ToList();

        var timeSlots = request.TimeSlots
            .OrderBy(x => x.StartTime)
            .ToList();

        // --------------------------------------------------------
        // LOAD DATA ONCE
        // --------------------------------------------------------

        var groups = await _context.Groups
            .AsNoTracking()
            .Where(g => selectedGroupIds.Contains(g.Id))
            .ToListAsync();

        var groupSubjects = await _context.GroupSubjects
            .AsNoTracking()
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject)
                .ThenInclude(s => s!.ClassroomCategoryRequirements)
            .Where(gs => selectedGroupIds.Contains(gs.GroupId))
            .ToListAsync();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
            .ToListAsync();

        var teachers = await _context.Teachers
            .AsNoTracking()
            .ToListAsync();

        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .ToListAsync();

        var existingSchedules = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.DayOfWeek.HasValue &&
                s.StartTime.HasValue &&
                s.EndTime.HasValue)
            .ToListAsync();

        // --------------------------------------------------------
        // VALIDATE SELECTED GROUPS
        // --------------------------------------------------------

        var existingGroupIds = groups
            .Select(g => g.Id)
            .ToHashSet();

        foreach (var groupId in selectedGroupIds)
        {
            if (!existingGroupIds.Contains(groupId))
            {
                result.AddError(
                    $"Группа с ID {groupId} не найдена."
                );
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // CREATE LESSON TASKS
        // --------------------------------------------------------

        var lessonTasks = new List<LessonTask>();

        foreach (var group in groups)
        {
            var groupLoads = groupSubjects
                .Where(gs => gs.GroupId == group.Id)
                .ToList();

            if (groupLoads.Count == 0)
            {
                result.AddError(
                    $"Для группы «{group.Name}» не назначены предметы."
                );

                continue;
            }

            foreach (var groupSubject in groupLoads)
            {
                if (groupSubject.Subject == null)
                {
                    result.AddError(
                        $"У группы «{group.Name}» найден предмет, " +
                        "который не существует."
                    );

                    continue;
                }

                if (groupSubject.WeeklyLessons <= 0)
                {
                    result.AddError(
                        $"Для предмета «{groupSubject.Subject.Name}» " +
                        $"группы «{group.Name}» указано некорректное " +
                        "количество занятий в неделю."
                    );

                    continue;
                }

                for (var lessonNumber = 1;
                     lessonNumber <= groupSubject.WeeklyLessons;
                     lessonNumber++)
                {
                    lessonTasks.Add(
                        new LessonTask
                        {
                            GroupId = group.Id,
                            GroupName = group.Name,
                            StudentCount = group.StudentCount,

                            SubjectId = groupSubject.Subject.Id,
                            SubjectName = groupSubject.Subject.Name,

                            Subject = groupSubject.Subject,

                            LessonNumber = lessonNumber
                        }
                    );
                }
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        if (lessonTasks.Count == 0)
        {
            result.AddError(
                "Не найдено ни одного занятия для генерации."
            );

            return result;
        }

        // --------------------------------------------------------
        // CHECK TOTAL CAPACITY
        // --------------------------------------------------------

        var availableSlotsCount =
            selectedDays.Count * timeSlots.Count;

        foreach (var group in groups)
        {
            var requiredLessons = lessonTasks
                .Count(x => x.GroupId == group.Id);

            if (requiredLessons > availableSlotsCount)
            {
                result.AddError(
                    $"Для группы «{group.Name}» требуется " +
                    $"{requiredLessons} занятий, но доступно только " +
                    $"{availableSlotsCount} временных слотов."
                );
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // TEACHER CHECK
        // --------------------------------------------------------

        foreach (var task in lessonTasks)
        {
            var candidates = GetTeacherCandidates(
                task,
                teachers);

            if (candidates.Count == 0)
            {
                result.AddError(
                    $"Для предмета «{task.SubjectName}» " +
                    $"не найден преподаватель."
                );
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // CLASSROOM CHECK
        // --------------------------------------------------------

        foreach (var task in lessonTasks)
        {
            var group = groups.First(
                x => x.Id == task.GroupId);

            var candidates = GetClassroomCandidates(
                task,
                group,
                classrooms);

            if (candidates.Count == 0)
            {
                result.AddError(
                    $"Для занятия «{task.SubjectName}» " +
                    $"группы «{task.GroupName}» " +
                    "не найдена подходящая аудитория."
                );
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // ORDER LESSONS
        // --------------------------------------------------------

        var orderedLessons = OrderLessons(
            lessonTasks,
            request.DistributeLessons);

        // --------------------------------------------------------
        // CREATE SLOT LIST
        // --------------------------------------------------------

        var slots = new List<GenerationSlot>();

        foreach (var day in selectedDays)
        {
            for (var i = 0; i < timeSlots.Count; i++)
            {
                slots.Add(
                    new GenerationSlot
                    {
                        DayOfWeek = day,
                        StartTime = timeSlots[i].StartTime,
                        EndTime = timeSlots[i].EndTime,
                        SlotIndex = i
                    }
                );
            }
        }

        // --------------------------------------------------------
        // GENERATION STATE
        // --------------------------------------------------------

        var generated = new List<GeneratedCandidate>();

        var searchState = new SearchState
        {
            NodesVisited = 0,
            Failed = false
        };

        // --------------------------------------------------------
        // BACKTRACKING
        // --------------------------------------------------------

        var success = TryGenerate(
            index: 0,
            lessons: orderedLessons,
            slots: slots,
            groups: groups,
            teachers: teachers,
            classrooms: classrooms,
            existingSchedules: existingSchedules,
            generated: generated,
            request: request,
            state: searchState);

        if (!success)
        {
            result.AddError(
                searchState.NodesVisited >= MaxSearchNodes
                    ? "Не удалось построить расписание в допустимое количество попыток. " +
                      "Попробуйте уменьшить количество занятий, увеличить число дней " +
                      "или добавить временные интервалы."
                    : "Не удалось построить расписание без конфликтов. " +
                      "Попробуйте изменить дни, интервалы или ограничения."
            );

            return result;
        }

        // --------------------------------------------------------
        // CONVERT RESULT
        // --------------------------------------------------------

        foreach (var item in generated
                     .OrderBy(x => x.DayOfWeek)
                     .ThenBy(x => x.StartTime)
                     .ThenBy(x => x.GroupName))
        {
            result.GeneratedItems.Add(
                new GeneratedScheduleItem
                {
                    GroupId = item.GroupId,
                    GroupName = item.GroupName,

                    SubjectId = item.SubjectId,
                    SubjectName = item.SubjectName,

                    TeacherId = item.TeacherId,
                    TeacherName = item.TeacherName,

                    ClassroomId = item.ClassroomId,
                    ClassroomName = item.ClassroomName,

                    DayOfWeek = item.DayOfWeek,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,

                    LessonNumber = item.LessonNumber
                }
            );
        }

        result.IsSuccess = true;

        return result;
    }

    // ============================================================
    // REQUEST VALIDATION
    // ============================================================

    private static List<string> ValidateRequest(
        ScheduleGenerationRequest request)
    {
        var errors = new List<string>();

        if (request.SelectedGroupIds == null ||
            request.SelectedGroupIds.Count == 0)
        {
            errors.Add("Выберите хотя бы одну группу.");
        }

        if (request.Days == null ||
            request.Days.Count == 0)
        {
            errors.Add("Выберите хотя бы один день недели.");
        }

        if (request.TimeSlots == null ||
            request.TimeSlots.Count == 0)
        {
            errors.Add(
                "Добавьте хотя бы один временной интервал."
            );
        }
        else
        {
            for (var i = 0;
                 i < request.TimeSlots.Count;
                 i++)
            {
                var slot = request.TimeSlots[i];

                if (slot.StartTime >= slot.EndTime)
                {
                    errors.Add(
                        $"Временной интервал №{i + 1} некорректен."
                    );
                }
            }

            for (var i = 0;
                 i < request.TimeSlots.Count;
                 i++)
            {
                for (var j = i + 1;
                     j < request.TimeSlots.Count;
                     j++)
                {
                    var first = request.TimeSlots[i];
                    var second = request.TimeSlots[j];

                    if (first.StartTime < second.EndTime &&
                        first.EndTime > second.StartTime)
                    {
                        errors.Add(
                            $"Временные интервалы №{i + 1} и " +
                            $"№{j + 1} пересекаются."
                        );
                    }
                }
            }
        }

        if (request.MaxLessonsPerDay < 1 ||
            request.MaxLessonsPerDay > 10)
        {
            errors.Add(
                "Максимальное количество пар в день должно быть от 1 до 10."
            );
        }

        if (request.MaxConsecutiveLessons < 1 ||
            request.MaxConsecutiveLessons > 10)
        {
            errors.Add(
                "Максимальное количество пар подряд должно быть от 1 до 10."
            );
        }

        return errors;
    }

    // ============================================================
    // ORDER LESSONS
    // ============================================================

    private static List<LessonTask> OrderLessons(
        List<LessonTask> lessons,
        bool distributeLessons)
    {
        /*
         * Сначала ставим более сложные занятия:
         *
         * 1. предметы с меньшим количеством преподавателей;
         * 2. предметы с более строгими требованиями;
         * 3. большие группы.
         *
         * Само количество преподавателей здесь не загружаем
         * отдельным запросом — предварительная сортировка
         * выполняется позже непосредственно в поиске.
         */

        if (!distributeLessons)
        {
            return lessons
                .OrderByDescending(x => x.StudentCount)
                .ThenBy(x => x.SubjectName)
                .ThenBy(x => x.GroupName)
                .ThenBy(x => x.LessonNumber)
                .ToList();
        }

        /*
         * При распределении одинаковые предметы одной группы
         * стараемся разнести по поиску.
         */

        return lessons
            .OrderByDescending(x => x.StudentCount)
            .ThenBy(x => x.SubjectName)
            .ThenBy(x => x.GroupName)
            .ThenBy(x => x.LessonNumber)
            .ToList();
    }

    // ============================================================
    // BACKTRACKING
    // ============================================================

    private bool TryGenerate(
        int index,
        List<LessonTask> lessons,
        List<GenerationSlot> slots,
        List<Group> groups,
        List<Teacher> teachers,
        List<Classroom> classrooms,
        List<Schedule> existingSchedules,
        List<GeneratedCandidate> generated,
        ScheduleGenerationRequest request,
        SearchState state)
    {
        if (index >= lessons.Count)
        {
            return true;
        }

        state.NodesVisited++;

        if (state.NodesVisited > MaxSearchNodes)
        {
            return false;
        }

        var remainingLessons = lessons
            .Skip(index)
            .ToList();

        var task = SelectMostConstrainedLesson(
            remainingLessons,
            slots,
            groups,
            teachers,
            classrooms,
            existingSchedules,
            generated,
            request);

        if (task == null)
        {
            return false;
        }

        /*
         * Меняем task с текущей позицией.
         * Это позволяет сначала обрабатывать самые сложные занятия.
         */

        var taskIndex = lessons.IndexOf(task);

        if (taskIndex != index)
        {
            (lessons[index], lessons[taskIndex]) =
                (lessons[taskIndex], lessons[index]);

            task = lessons[index];
        }

        var group = groups.First(
            x => x.Id == task.GroupId);

        var teacherCandidates = GetTeacherCandidates(
            task,
            teachers);

        var classroomCandidates = GetClassroomCandidates(
            task,
            group,
            classrooms);

        if (teacherCandidates.Count == 0 ||
            classroomCandidates.Count == 0)
        {
            return false;
        }

        var slotCandidates = GetSlotCandidates(
            task,
            slots,
            existingSchedules,
            generated,
            request);

        foreach (var slot in slotCandidates)
        {
            if (!CanPlaceGroup(
                    task,
                    slot,
                    generated,
                    existingSchedules,
                    request))
            {
                continue;
            }

            foreach (var teacher in teacherCandidates)
            {
                if (!CanPlaceTeacher(
                        teacher.Id,
                        slot,
                        generated,
                        existingSchedules))
                {
                    continue;
                }

                var orderedClassrooms = OrderClassrooms(
                    classroomCandidates,
                    task,
                    group,
                    request.UseClassroomRecommendations);

                foreach (var classroom in orderedClassrooms)
                {
                    if (!CanPlaceClassroom(
                            classroom.Id,
                            slot,
                            generated,
                            existingSchedules))
                    {
                        continue;
                    }

                    var candidate =
                        new GeneratedCandidate
                        {
                            GroupId = group.Id,
                            GroupName = group.Name,

                            SubjectId = task.SubjectId,
                            SubjectName = task.SubjectName,

                            TeacherId = teacher.Id,
                            TeacherName = teacher.FullName,

                            ClassroomId = classroom.Id,
                            ClassroomName = classroom.Name,

                            DayOfWeek = slot.DayOfWeek,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,

                            LessonNumber = task.LessonNumber,

                            SlotIndex = slot.SlotIndex
                        };

                    generated.Add(candidate);

                    if (TryGenerate(
                            index + 1,
                            lessons,
                            slots,
                            groups,
                            teachers,
                            classrooms,
                            existingSchedules,
                            generated,
                            request,
                            state))
                    {
                        return true;
                    }

                    generated.Remove(candidate);

                    if (state.NodesVisited > MaxSearchNodes)
                    {
                        return false;
                    }
                }
            }
        }

        return false;
    }

    // ============================================================
    // MOST CONSTRAINED LESSON
    // ============================================================

    private LessonTask? SelectMostConstrainedLesson(
        List<LessonTask> lessons,
        List<GenerationSlot> slots,
        List<Group> groups,
        List<Teacher> teachers,
        List<Classroom> classrooms,
        List<Schedule> existingSchedules,
        List<GeneratedCandidate> generated,
        ScheduleGenerationRequest request)
    {
        LessonTask? selected = null;
        var smallestCandidateCount = int.MaxValue;

        foreach (var lesson in lessons)
        {
            var group = groups.First(
                x => x.Id == lesson.GroupId);

            var teacherCount = GetTeacherCandidates(
                lesson,
                teachers).Count;

            var classroomCount = GetClassroomCandidates(
                lesson,
                group,
                classrooms).Count;

            if (teacherCount == 0 ||
                classroomCount == 0)
            {
                return lesson;
            }

            var slotCount = GetSlotCandidates(
                lesson,
                slots,
                existingSchedules,
                generated,
                request).Count;

            var total =
                Math.Max(1, teacherCount) *
                Math.Max(1, classroomCount) *
                Math.Max(1, slotCount);

            if (total < smallestCandidateCount)
            {
                smallestCandidateCount = total;
                selected = lesson;
            }
        }

        return selected;
    }

    // ============================================================
    // TEACHERS
    // ============================================================

    private static List<Teacher> GetTeacherCandidates(
        LessonTask task,
        List<Teacher> teachers)
    {
        return teachers
            .Where(t => t.SubjectId == task.SubjectId)
            .OrderBy(t => t.FullName)
            .ToList();
    }

    // ============================================================
    // CLASSROOMS
    // ============================================================

    private static List<Classroom> GetClassroomCandidates(
        LessonTask task,
        Group group,
        List<Classroom> classrooms)
    {
        var requiredCategoryIds = task.Subject
            .ClassroomCategoryRequirements
            .Select(x => x.ClassroomCategoryId)
            .Distinct()
            .ToHashSet();

        return classrooms
            .Where(c =>
                c.Capacity >= group.StudentCount)
            .Where(c =>
                requiredCategoryIds.Count == 0 ||
                requiredCategoryIds.Contains(
                    c.ClassroomCategoryId))
            .OrderBy(c => c.Capacity)
            .ThenBy(c => c.Name)
            .ToList();
    }

    // ============================================================
    // SLOTS
    // ============================================================

    private static List<GenerationSlot> GetSlotCandidates(
        LessonTask task,
        List<GenerationSlot> slots,
        List<Schedule> existingSchedules,
        List<GeneratedCandidate> generated,
        ScheduleGenerationRequest request)
    {
        var candidates = new List<GenerationSlot>();

        foreach (var slot in slots)
        {
            if (!CanPlaceGroup(
                    task,
                    slot,
                    generated,
                    existingSchedules,
                    request))
            {
                continue;
            }

            candidates.Add(slot);
        }

        if (request.DistributeLessons)
        {
            candidates = candidates
                .OrderBy(slot =>
                    CountGroupLessonsOnDay(
                        task.GroupId,
                        slot.DayOfWeek,
                        generated))
                .ThenBy(slot =>
                    CountGroupLessonsInSlotRange(
                        task.GroupId,
                        slot,
                        generated))
                .ThenBy(slot => slot.DayOfWeek)
                .ThenBy(slot => slot.StartTime)
                .ToList();
        }
        else
        {
            candidates = candidates
                .OrderBy(slot => slot.DayOfWeek)
                .ThenBy(slot => slot.StartTime)
                .ToList();
        }

        return candidates;
    }

    // ============================================================
    // GROUP CONFLICTS
    // ============================================================

    private static bool CanPlaceGroup(
        LessonTask task,
        GenerationSlot slot,
        List<GeneratedCandidate> generated,
        List<Schedule> existingSchedules,
        ScheduleGenerationRequest request)
    {
        var existingGroupSchedules = existingSchedules
            .Where(s =>
                s.GroupId == task.GroupId &&
                s.DayOfWeek == slot.DayOfWeek &&
                s.StartTime.HasValue &&
                s.EndTime.HasValue)
            .ToList();

        foreach (var existing in existingGroupSchedules)
        {
            if (Overlaps(
                    existing.StartTime!.Value,
                    existing.EndTime!.Value,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        foreach (var item in generated)
        {
            if (item.GroupId != task.GroupId ||
                item.DayOfWeek != slot.DayOfWeek)
            {
                continue;
            }

            if (Overlaps(
                    item.StartTime,
                    item.EndTime,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        var lessonsToday = CountGroupLessonsOnDay(
            task.GroupId,
            slot.DayOfWeek,
            generated);

        var existingLessonsToday = existingSchedules
            .Count(s =>
                s.GroupId == task.GroupId &&
                s.DayOfWeek == slot.DayOfWeek);

        if (lessonsToday + existingLessonsToday >=
            request.MaxLessonsPerDay)
        {
            return false;
        }

        if (!CanPlaceConsecutive(
                task.GroupId,
                slot,
                generated,
                existingSchedules,
                request.MaxConsecutiveLessons))
        {
            return false;
        }

        return true;
    }

    // ============================================================
    // TEACHER CONFLICTS
    // ============================================================

    private static bool CanPlaceTeacher(
        int teacherId,
        GenerationSlot slot,
        List<GeneratedCandidate> generated,
        List<Schedule> existingSchedules)
    {
        foreach (var existing in existingSchedules)
        {
            if (existing.TeacherId != teacherId ||
                existing.DayOfWeek != slot.DayOfWeek ||
                !existing.StartTime.HasValue ||
                !existing.EndTime.HasValue)
            {
                continue;
            }

            if (Overlaps(
                    existing.StartTime.Value,
                    existing.EndTime.Value,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        foreach (var item in generated)
        {
            if (item.TeacherId != teacherId ||
                item.DayOfWeek != slot.DayOfWeek)
            {
                continue;
            }

            if (Overlaps(
                    item.StartTime,
                    item.EndTime,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        return true;
    }

    // ============================================================
    // CLASSROOM CONFLICTS
    // ============================================================

    private static bool CanPlaceClassroom(
        int classroomId,
        GenerationSlot slot,
        List<GeneratedCandidate> generated,
        List<Schedule> existingSchedules)
    {
        foreach (var existing in existingSchedules)
        {
            if (existing.ClassroomId != classroomId ||
                existing.DayOfWeek != slot.DayOfWeek ||
                !existing.StartTime.HasValue ||
                !existing.EndTime.HasValue)
            {
                continue;
            }

            if (Overlaps(
                    existing.StartTime.Value,
                    existing.EndTime.Value,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        foreach (var item in generated)
        {
            if (item.ClassroomId != classroomId ||
                item.DayOfWeek != slot.DayOfWeek)
            {
                continue;
            }

            if (Overlaps(
                    item.StartTime,
                    item.EndTime,
                    slot.StartTime,
                    slot.EndTime))
            {
                return false;
            }
        }

        return true;
    }

    // ============================================================
    // CONSECUTIVE LESSONS
    // ============================================================

    private static bool CanPlaceConsecutive(
        int groupId,
        GenerationSlot slot,
        List<GeneratedCandidate> generated,
        List<Schedule> existingSchedules,
        int maxConsecutive)
    {
        if (maxConsecutive <= 0)
        {
            return true;
        }

        var occupied = new HashSet<int>();

        foreach (var item in generated)
        {
            if (item.GroupId == groupId &&
                item.DayOfWeek == slot.DayOfWeek)
            {
                occupied.Add(item.SlotIndex);
            }
        }

        /*
         * Существующие расписания не имеют SlotIndex.
         * Поэтому здесь проверяем только непосредственное
         * количество уже сгенерированных соседних слотов.
         *
         * Конфликты с существующей БД при этом всё равно
         * полностью проверяются выше.
         */

        occupied.Add(slot.SlotIndex);

        var consecutive = 1;

        var left = slot.SlotIndex - 1;

        while (occupied.Contains(left))
        {
            consecutive++;
            left--;
        }

        var right = slot.SlotIndex + 1;

        while (occupied.Contains(right))
        {
            consecutive++;
            right++;
        }

        return consecutive <= maxConsecutive;
    }

    // ============================================================
    // CLASSROOM ORDER
    // ============================================================

    private static List<Classroom> OrderClassrooms(
        List<Classroom> classrooms,
        LessonTask task,
        Group group,
        bool useRecommendations)
    {
        var requiredCategoryIds = task.Subject
            .ClassroomCategoryRequirements
            .Select(x => x.ClassroomCategoryId)
            .Distinct()
            .ToHashSet();

        if (!useRecommendations)
        {
            return classrooms
                .OrderBy(c => c.Name)
                .ToList();
        }

        /*
         * Предпочитаем аудиторию:
         *
         * 1. подходящей категории;
         * 2. с минимальным достаточным количеством мест.
         *
         * Таким образом большая аудитория не занимает место,
         * если есть подходящая небольшая.
         */

        return classrooms
            .OrderByDescending(c =>
                requiredCategoryIds.Count == 0 ||
                requiredCategoryIds.Contains(
                    c.ClassroomCategoryId))
            .ThenBy(c =>
                Math.Max(0, c.Capacity - group.StudentCount))
            .ThenBy(c => c.Capacity)
            .ThenBy(c => c.Name)
            .ToList();
    }

    // ============================================================
    // DISTRIBUTION HELPERS
    // ============================================================

    private static int CountGroupLessonsOnDay(
        int groupId,
        DayOfWeek day,
        List<GeneratedCandidate> generated)
    {
        return generated.Count(x =>
            x.GroupId == groupId &&
            x.DayOfWeek == day);
    }

    private static int CountGroupLessonsInSlotRange(
        int groupId,
        GenerationSlot slot,
        List<GeneratedCandidate> generated)
    {
        return generated.Count(x =>
            x.GroupId == groupId &&
            x.DayOfWeek == slot.DayOfWeek &&
            (
                x.StartTime == slot.StartTime ||
                x.EndTime == slot.EndTime
            ));
    }

    // ============================================================
    // OVERLAP
    // ============================================================

    private static bool Overlaps(
        TimeSpan firstStart,
        TimeSpan firstEnd,
        TimeSpan secondStart,
        TimeSpan secondEnd)
    {
        return firstStart < secondEnd &&
               firstEnd > secondStart;
    }

    // ============================================================
    // INTERNAL TYPES
    // ============================================================

    private sealed class LessonTask
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int StudentCount { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public Subject Subject { get; set; } = null!;

        public int LessonNumber { get; set; }
    }

    private sealed class GenerationSlot
    {
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int SlotIndex { get; set; }
    }

    private sealed class GeneratedCandidate
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public int ClassroomId { get; set; }

        public string ClassroomName { get; set; } = string.Empty;

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int LessonNumber { get; set; }

        public int SlotIndex { get; set; }
    }

    private sealed class SearchState
    {
        public int NodesVisited { get; set; }

        public bool Failed { get; set; }
    }
}