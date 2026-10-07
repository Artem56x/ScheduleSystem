using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using ScheduleSystem.Models.Generator;

namespace ScheduleSystem.Services;

public class ScheduleGeneratorService
{
    private readonly ApplicationDbContext _context;

    private const int MaxSearchNodes = 150_000;

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
        // REQUEST VALIDATION
        // --------------------------------------------------------

        var requestErrors = ValidateRequest(request);

        if (requestErrors.Count > 0)
        {
            foreach (var error in requestErrors)
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
            .OrderBy(x => x)
            .ToList();

        var timeSlots = request.TimeSlots
            .OrderBy(x => x.StartTime)
            .ToList();

        // --------------------------------------------------------
        // LOAD GROUPS
        // --------------------------------------------------------

        var groups = await _context.Groups
            .AsNoTracking()
            .Where(g => selectedGroupIds.Contains(g.Id))
            .ToListAsync();

        var existingGroupIds = groups
            .Select(g => g.Id)
            .ToHashSet();

        foreach (var groupId in selectedGroupIds)
        {
            if (!existingGroupIds.Contains(groupId))
            {
                result.AddError(
                    $"Группа с ID {groupId} не найдена.");
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // LOAD GROUP SUBJECTS
        // --------------------------------------------------------

        var groupSubjects = await _context.GroupSubjects
            .AsNoTracking()
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject)
                .ThenInclude(s => s!.ClassroomCategoryRequirements)
            .Where(gs => selectedGroupIds.Contains(gs.GroupId))
            .ToListAsync();

        // --------------------------------------------------------
        // LOAD TEACHERS
        // --------------------------------------------------------
        //
        // Один преподаватель может вести несколько предметов.
        // Связь хранится через TeacherSubjects.
        // --------------------------------------------------------

        var teachers = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
            .ToListAsync();

        // --------------------------------------------------------
        // LOAD CLASSROOMS
        // --------------------------------------------------------

        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .ToListAsync();

        // --------------------------------------------------------
        // LOAD EXISTING SCHEDULE
        // --------------------------------------------------------
        //
        // ВАЖНО:
        //
        // Расписание выбранных групп в выбранные дни НЕ загружаем,
        // потому что при подтверждении оно будет удалено и заменено.
        //
        // Расписание других групп продолжает блокировать:
        // - преподавателя;
        // - аудиторию;
        // - группу, если она не входит в выбранные группы.
        //
        // Расписание выбранных групп в другие дни также не влияет,
        // поскольку генерация идёт только по выбранным дням.
        // --------------------------------------------------------

        var existingSchedules = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.DayOfWeek.HasValue &&
                s.StartTime.HasValue &&
                s.EndTime.HasValue &&
                selectedDays.Contains(s.DayOfWeek.Value) &&
                !selectedGroupIds.Contains(s.GroupId))
            .ToListAsync();

        // --------------------------------------------------------
        // CREATE LESSON TASKS
        // --------------------------------------------------------

        var lessonTasks = BuildLessonTasks(
            groups,
            groupSubjects,
            result);

        if (result.HasErrors)
        {
            return result;
        }

        if (lessonTasks.Count == 0)
        {
            result.AddError(
                "Не найдено ни одного занятия для генерации.");

            return result;
        }

        // --------------------------------------------------------
        // CAPACITY CHECK
        // --------------------------------------------------------

        var availableSlotsPerGroup =
            selectedDays.Count * timeSlots.Count;

        foreach (var group in groups)
        {
            var requiredLessons = lessonTasks.Count(
                x => x.GroupId == group.Id);

            if (requiredLessons > availableSlotsPerGroup)
            {
                result.AddError(
                    $"Для группы «{group.Name}» требуется " +
                    $"{requiredLessons} занятий, но доступно только " +
                    $"{availableSlotsPerGroup} временных слотов.");
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
            if (GetTeacherCandidates(task, teachers).Count == 0)
            {
                result.AddError(
                    $"Для предмета «{task.SubjectName}» " +
                    "не найден преподаватель.");
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
                g => g.Id == task.GroupId);

            if (GetClassroomCandidates(
                    task,
                    group,
                    classrooms).Count == 0)
            {
                result.AddError(
                    $"Для занятия «{task.SubjectName}» " +
                    $"группы «{task.GroupName}» " +
                    "не найдена подходящая аудитория.");
            }
        }

        if (result.HasErrors)
        {
            return result;
        }

        // --------------------------------------------------------
        // BUILD SLOTS
        // --------------------------------------------------------

        var slots = BuildSlots(
            selectedDays,
            timeSlots);

        // --------------------------------------------------------
        // ORDER LESSONS
        // --------------------------------------------------------

        var orderedLessons = OrderLessons(
            lessonTasks,
            groups,
            teachers,
            classrooms,
            slots,
            existingSchedules,
            request);

        // --------------------------------------------------------
        // BACKTRACKING GENERATION
        // --------------------------------------------------------

        var generated = new List<GeneratedCandidate>();

        var searchState = new SearchState();

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
                      "Попробуйте увеличить количество дней или временных интервалов."
                    : "Не удалось построить расписание без конфликтов. " +
                      "Попробуйте изменить нагрузку, дни или ограничения.");

            return result;
        }

        // --------------------------------------------------------
        // FINAL VALIDATION
        // --------------------------------------------------------

        var finalErrors = ValidateGeneratedSchedule(
            generated,
            lessonTasks,
            groups,
            teachers,
            classrooms,
            existingSchedules,
            request,
            timeSlots);

        if (finalErrors.Count > 0)
        {
            foreach (var error in finalErrors)
            {
                result.AddError(error);
            }

            return result;
        }

        // --------------------------------------------------------
        // CONVERT RESULT
        // --------------------------------------------------------

        foreach (var item in generated
                     .OrderBy(x => x.DayOfWeek)
                     .ThenBy(x => x.StartTime)
                     .ThenBy(x => x.GroupName)
                     .ThenBy(x => x.SubjectName))
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
                });
        }

        result.IsSuccess = true;

        return result;
    }

    // ============================================================
    // VALIDATE BEFORE SAVE
    // ============================================================

    public async Task<List<string>> ValidateBeforeSaveAsync(
        ScheduleGenerationPreviewViewModel preview)
    {
        var errors = new List<string>();

        if (preview == null)
        {
            errors.Add("Предпросмотр расписания не найден.");
            return errors;
        }

        if (preview.Items == null ||
            preview.Items.Count == 0)
        {
            errors.Add(
                "Предпросмотр не содержит ни одного занятия.");

            return errors;
        }

        var selectedGroupIds = preview.SelectedGroupIds
            .Distinct()
            .ToHashSet();

        var selectedDays = preview.SelectedDays
            .Distinct()
            .ToHashSet();

        var timeSlots = preview.TimeSlots
            .OrderBy(x => x.StartTime)
            .ToList();

        if (selectedGroupIds.Count == 0)
        {
            errors.Add("В предпросмотре не выбрана ни одна группа.");
        }

        if (selectedDays.Count == 0)
        {
            errors.Add("В предпросмотре не выбран ни один день.");
        }

        if (timeSlots.Count == 0)
        {
            errors.Add(
                "В предпросмотре не найдено ни одного временного интервала.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        // --------------------------------------------------------
        // CURRENT GROUPS
        // --------------------------------------------------------

        var groups = await _context.Groups
            .AsNoTracking()
            .Where(g => selectedGroupIds.Contains(g.Id))
            .ToListAsync();

        var groupById = groups.ToDictionary(g => g.Id);

        foreach (var groupId in selectedGroupIds)
        {
            if (!groupById.ContainsKey(groupId))
            {
                errors.Add(
                    $"Группа с ID {groupId} больше не существует.");
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        // --------------------------------------------------------
        // CURRENT GROUP SUBJECTS
        // --------------------------------------------------------

        var groupSubjects = await _context.GroupSubjects
            .AsNoTracking()
            .Include(gs => gs.Subject)
                .ThenInclude(s => s!.ClassroomCategoryRequirements)
            .Where(gs => selectedGroupIds.Contains(gs.GroupId))
            .ToListAsync();

        var groupSubjectMap = groupSubjects
            .GroupBy(gs => new
            {
                gs.GroupId,
                gs.SubjectId
            })
            .ToDictionary(
                g => (g.Key.GroupId, g.Key.SubjectId),
                g => g.First());

        // --------------------------------------------------------
        // CURRENT TEACHERS
        // --------------------------------------------------------

        var teachers = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
            .ToListAsync();

        var teacherById = teachers.ToDictionary(
            t => t.Id);

        // --------------------------------------------------------
        // CURRENT CLASSROOMS
        // --------------------------------------------------------

        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .ToListAsync();

        var classroomById = classrooms.ToDictionary(
            c => c.Id);

        // --------------------------------------------------------
        // PROTECTED EXISTING SCHEDULES
        // --------------------------------------------------------
        //
        // Выбранные группы в выбранные дни будут заменены,
        // поэтому они НЕ считаются конфликтующими.
        //
        // Остальные записи защищены и должны продолжать
        // блокировать генерацию.
        // --------------------------------------------------------

        var protectedSchedules = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.DayOfWeek.HasValue &&
                s.StartTime.HasValue &&
                s.EndTime.HasValue &&
                selectedDays.Contains(s.DayOfWeek.Value) &&
                !selectedGroupIds.Contains(s.GroupId))
            .ToListAsync();

        // --------------------------------------------------------
        // PREVIEW ITEM VALIDATION
        // --------------------------------------------------------

        foreach (var item in preview.Items)
        {
            if (!selectedGroupIds.Contains(item.GroupId))
            {
                errors.Add(
                    $"Предпросмотр содержит занятие группы «{item.GroupName}», " +
                    "которая не выбрана для генерации.");
            }

            if (!selectedDays.Contains(item.DayOfWeek))
            {
                errors.Add(
                    $"Занятие «{item.SubjectName}» группы «{item.GroupName}» " +
                    "находится вне выбранных дней.");
            }

            if (item.StartTime >= item.EndTime)
            {
                errors.Add(
                    $"Некорректное время занятия «{item.SubjectName}» " +
                    $"для группы «{item.GroupName}».");
            }

            if (!groupById.TryGetValue(
                    item.GroupId,
                    out var group))
            {
                continue;
            }

            // ----------------------------------------------------
            // GROUP SUBJECT
            // ----------------------------------------------------

            if (!groupSubjectMap.TryGetValue(
                    (item.GroupId, item.SubjectId),
                    out var groupSubject))
            {
                errors.Add(
                    $"Предмет «{item.SubjectName}» больше не назначен " +
                    $"группе «{group.Name}».");

                continue;
            }

            if (groupSubject.Subject == null)
            {
                errors.Add(
                    $"Предмет с ID {item.SubjectId} больше не существует.");

                continue;
            }

            // ----------------------------------------------------
            // КУРС ГРУППЫ / КУРС ПРЕДМЕТА
            // ----------------------------------------------------

            if (group.Course != groupSubject.Subject.Course)
            {
                errors.Add(
                    $"Предмет «{groupSubject.Subject.Name}» относится " +
                    $"к {groupSubject.Subject.Course} курсу, " +
                    $"а группа «{group.Name}» относится к {group.Course} курсу. " +
                    "Предпросмотр расписания больше не соответствует учебной нагрузке.");

                continue;
            }

            // ----------------------------------------------------
            // TEACHER
            // ----------------------------------------------------

            if (!teacherById.TryGetValue(
                    item.TeacherId,
                    out var teacher))
            {
                errors.Add(
                    $"Преподаватель «{item.TeacherName}» больше не существует.");
            }
            else if (!teacher.TeacherSubjects.Any(
                         ts => ts.SubjectId == item.SubjectId))
            {
                errors.Add(
                    $"Преподаватель «{teacher.FullName}» больше не может " +
                    $"вести предмет «{item.SubjectName}».");
            }

            // ----------------------------------------------------
            // CLASSROOM
            // ----------------------------------------------------

            if (!classroomById.TryGetValue(
                    item.ClassroomId,
                    out var classroom))
            {
                errors.Add(
                    $"Аудитория «{item.ClassroomName}» больше не существует.");

                continue;
            }

            if (classroom.Capacity < group.StudentCount)
            {
                errors.Add(
                    $"Аудитория «{classroom.Name}» не вмещает " +
                    $"группу «{group.Name}».");
            }

            var requiredCategories = groupSubject.Subject
                .ClassroomCategoryRequirements
                .Select(x => x.ClassroomCategoryId)
                .Distinct()
                .ToHashSet();

            if (requiredCategories.Count > 0 &&
                !requiredCategories.Contains(
                    classroom.ClassroomCategoryId))
            {
                errors.Add(
                    $"Аудитория «{classroom.Name}» не соответствует " +
                    $"требованиям предмета «{item.SubjectName}».");
            }
        }

        // --------------------------------------------------------
        // LOAD COUNTS
        // --------------------------------------------------------

        foreach (var group in groups)
        {
            var currentLoads = groupSubjects
                .Where(gs => gs.GroupId == group.Id)
                .ToList();

            foreach (var groupSubject in currentLoads)
            {
                var actual = preview.Items.Count(item =>
                    item.GroupId == group.Id &&
                    item.SubjectId == groupSubject.SubjectId);

                if (actual != groupSubject.WeeklyLessons)
                {
                    var subjectName =
                        groupSubject.Subject?.Name ??
                        $"ID {groupSubject.SubjectId}";

                    errors.Add(
                        $"Нагрузка по предмету «{subjectName}» " +
                        $"для группы «{group.Name}» изменилась. " +
                        $"Ожидалось {groupSubject.WeeklyLessons}, " +
                        $"в предпросмотре {actual}.");
                }
            }
        }

        // --------------------------------------------------------
        // INTERNAL CONFLICTS
        // --------------------------------------------------------

        ValidatePreviewInternalConflicts(
            preview.Items,
            errors);

        // --------------------------------------------------------
        // MAX LESSONS PER DAY
        // --------------------------------------------------------

        foreach (var groupDay in preview.Items
                     .GroupBy(x => new
                     {
                         x.GroupId,
                         x.DayOfWeek
                     }))
        {
            if (groupDay.Count() >
                preview.MaxLessonsPerDay)
            {
                var item = groupDay.First();

                errors.Add(
                    $"Для группы «{item.GroupName}» на {GetDayName(item.DayOfWeek)} " +
                    $"запланировано {groupDay.Count()} занятий, " +
                    $"что превышает лимит {preview.MaxLessonsPerDay}.");
            }
        }

        // --------------------------------------------------------
        // CONSECUTIVE LESSONS
        // --------------------------------------------------------

        ValidatePreviewConsecutiveLessons(
            preview.Items,
            protectedSchedules,
            timeSlots,
            preview.MaxConsecutiveLessons,
            errors);

        // --------------------------------------------------------
        // CONFLICTS WITH PROTECTED SCHEDULE
        // --------------------------------------------------------

        foreach (var item in preview.Items)
        {
            foreach (var existing in protectedSchedules)
            {
                if (existing.DayOfWeek != item.DayOfWeek ||
                    !existing.StartTime.HasValue ||
                    !existing.EndTime.HasValue)
                {
                    continue;
                }

                if (!Overlaps(
                        existing.StartTime.Value,
                        existing.EndTime.Value,
                        item.StartTime,
                        item.EndTime))
                {
                    continue;
                }

                if (existing.GroupId == item.GroupId)
                {
                    errors.Add(
                        $"Группа «{item.GroupName}» уже занята " +
                        "в выбранное время.");
                }

                if (existing.TeacherId == item.TeacherId)
                {
                    errors.Add(
                        $"Преподаватель «{item.TeacherName}» уже занят " +
                        "в выбранное время.");
                }

                if (existing.ClassroomId == item.ClassroomId)
                {
                    errors.Add(
                        $"Аудитория «{item.ClassroomName}» уже занята " +
                        "в выбранное время.");
                }
            }
        }

        return errors
            .Distinct()
            .ToList();
    }

    // ============================================================
    // LESSON TASKS
    // ============================================================

    private static List<LessonTask> BuildLessonTasks(
        List<Group> groups,
        List<GroupSubject> groupSubjects,
        ScheduleGenerationResult result)
    {
        var tasks = new List<LessonTask>();

        foreach (var group in groups)
        {
            var loads = groupSubjects
                .Where(gs => gs.GroupId == group.Id)
                .ToList();

            if (loads.Count == 0)
            {
                result.AddError(
                    $"Для группы «{group.Name}» не назначены предметы.");

                continue;
            }

            foreach (var groupSubject in loads)
            {
                if (groupSubject.Subject == null)
                {
                    result.AddError(
                        $"У группы «{group.Name}» найден предмет, " +
                        "который не существует.");

                    continue;
                }

                // ----------------------------------------------------
                // КУРС ГРУППЫ / КУРС ПРЕДМЕТА
                // ----------------------------------------------------

                if (group.Course != groupSubject.Subject.Course)
                {
                    result.AddError(
                        $"Предмет «{groupSubject.Subject.Name}» относится " +
                        $"к {groupSubject.Subject.Course} курсу, " +
                        $"но группа «{group.Name}» относится к {group.Course} курсу. " +
                        "Предмет не может быть использован для этой группы.");

                    continue;
                }

                // ----------------------------------------------------
                // WEEKLY LOAD
                // ----------------------------------------------------

                if (groupSubject.WeeklyLessons <= 0)
                {
                    result.AddError(
                        $"Для предмета «{groupSubject.Subject.Name}» " +
                        $"группы «{group.Name}» указано некорректное " +
                        "количество занятий в неделю.");

                    continue;
                }

                for (var number = 1;
                     number <= groupSubject.WeeklyLessons;
                     number++)
                {
                    tasks.Add(
                        new LessonTask
                        {
                            GroupId = group.Id,
                            GroupName = group.Name,
                            StudentCount = group.StudentCount,

                            SubjectId = groupSubject.Subject.Id,
                            SubjectName = groupSubject.Subject.Name,

                            Subject = groupSubject.Subject,

                            LessonNumber = number
                        });
                }
            }
        }

        return tasks;
    }

    // ============================================================
    // REQUEST VALIDATION
    // ============================================================

    private static List<string> ValidateRequest(
        ScheduleGenerationRequest request)
    {
        var errors = new List<string>();

        if (request == null)
        {
            errors.Add("Запрос генерации не найден.");
            return errors;
        }

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
                "Добавьте хотя бы один временной интервал.");
        }
        else
        {
            for (var i = 0; i < request.TimeSlots.Count; i++)
            {
                var slot = request.TimeSlots[i];

                if (slot.StartTime >= slot.EndTime)
                {
                    errors.Add(
                        $"Временной интервал №{i + 1} некорректен.");
                }
            }

            for (var i = 0; i < request.TimeSlots.Count; i++)
            {
                for (var j = i + 1;
                     j < request.TimeSlots.Count;
                     j++)
                {
                    var first = request.TimeSlots[i];
                    var second = request.TimeSlots[j];

                    if (Overlaps(
                            first.StartTime,
                            first.EndTime,
                            second.StartTime,
                            second.EndTime))
                    {
                        errors.Add(
                            $"Временные интервалы №{i + 1} и " +
                            $"№{j + 1} пересекаются.");
                    }
                }
            }
        }

        if (request.MaxLessonsPerDay < 1 ||
            request.MaxLessonsPerDay > 10)
        {
            errors.Add(
                "Максимальное количество пар в день должно быть от 1 до 10.");
        }

        if (request.MaxConsecutiveLessons < 1 ||
            request.MaxConsecutiveLessons > 10)
        {
            errors.Add(
                "Максимальное количество пар подряд должно быть от 1 до 10.");
        }

        return errors
            .Distinct()
            .ToList();
    }

    // ============================================================
    // BUILD SLOTS
    // ============================================================

    private static List<GenerationSlot> BuildSlots(
        List<DayOfWeek> days,
        List<GenerationTimeSlot> timeSlots)
    {
        var slots = new List<GenerationSlot>();

        foreach (var day in days)
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
                    });
            }
        }

        return slots;
    }

    // ============================================================
    // LESSON ORDER
    // ============================================================

    private static List<LessonTask> OrderLessons(
        List<LessonTask> lessons,
        List<Group> groups,
        List<Teacher> teachers,
        List<Classroom> classrooms,
        List<GenerationSlot> slots,
        List<Schedule> existingSchedules,
        ScheduleGenerationRequest request)
    {
        return lessons
            .OrderByDescending(x =>
                GetTeacherCandidates(x, teachers).Count == 1)

            .ThenByDescending(x =>
                GetClassroomCandidates(
                    x,
                    groups.First(g => g.Id == x.GroupId),
                    classrooms).Count == 1)

            .ThenByDescending(x =>
                x.Subject.ClassroomCategoryRequirements.Count > 0)

            .ThenByDescending(x => x.StudentCount)

            .ThenBy(x =>
                GetTeacherCandidates(x, teachers).Count)

            .ThenBy(x =>
                GetClassroomCandidates(
                    x,
                    groups.First(g => g.Id == x.GroupId),
                    classrooms).Count)

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

        var remaining = lessons
            .Skip(index)
            .ToList();

        var task = SelectMostConstrainedLesson(
            remaining,
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

        var taskIndex = lessons.IndexOf(task);

        if (taskIndex != index)
        {
            (lessons[index], lessons[taskIndex]) =
                (lessons[taskIndex], lessons[index]);

            task = lessons[index];
        }

        var group = groups.First(
            g => g.Id == task.GroupId);

        var teacherCandidates = GetTeacherCandidates(
            task,
            teachers);

        var classroomCandidates = GetClassroomCandidates(
            task,
            group,
            classrooms);

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

            var orderedTeachers = OrderTeachers(
                teacherCandidates,
                slot,
                generated);

            foreach (var teacher in orderedTeachers)
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

                    var candidate = new GeneratedCandidate
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

                    if (state.NodesVisited >= MaxSearchNodes)
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

    private static LessonTask? SelectMostConstrainedLesson(
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

        var smallestScore = long.MaxValue;

        foreach (var lesson in lessons)
        {
            var group = groups.First(
                g => g.Id == lesson.GroupId);

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

            if (slotCount == 0)
            {
                return lesson;
            }

            var score =
                (long)teacherCount *
                classroomCount *
                slotCount;

            if (score < smallestScore)
            {
                smallestScore = score;
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
            .Where(t =>
                t.TeacherSubjects.Any(
                    ts => ts.SubjectId == task.SubjectId))
            .OrderBy(t => t.FullName)
            .ToList();
    }

    private static List<Teacher> OrderTeachers(
        List<Teacher> teachers,
        GenerationSlot slot,
        List<GeneratedCandidate> generated)
    {
        return teachers
            .OrderBy(t =>
                generated.Count(x =>
                    x.TeacherId == t.Id &&
                    x.DayOfWeek == slot.DayOfWeek))
            .ThenBy(t => t.FullName)
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

        return classrooms
            .OrderByDescending(c =>
                requiredCategoryIds.Count == 0 ||
                requiredCategoryIds.Contains(
                    c.ClassroomCategoryId))

            .ThenBy(c =>
                Math.Max(
                    0,
                    c.Capacity - group.StudentCount))

            .ThenBy(c => c.Capacity)

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
        var candidates = slots
            .Where(slot =>
                CanPlaceGroup(
                    task,
                    slot,
                    generated,
                    existingSchedules,
                    request))
            .ToList();

        if (request.DistributeLessons)
        {
            return candidates
                .OrderBy(slot =>
                    CountGroupLessonsOnDay(
                        task.GroupId,
                        slot.DayOfWeek,
                        generated))

                .ThenBy(slot =>
                    CountSubjectLessonsOnDay(
                        task.GroupId,
                        task.SubjectId,
                        slot.DayOfWeek,
                        generated))

                .ThenBy(slot =>
                    CountGroupLessonsBeforeSlot(
                        task.GroupId,
                        slot,
                        generated))

                .ThenBy(slot => slot.DayOfWeek)
                .ThenBy(slot => slot.StartTime)
                .ToList();
        }

        return candidates
            .OrderBy(slot => slot.DayOfWeek)
            .ThenBy(slot => slot.StartTime)
            .ToList();
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
        // Protected existing schedules.
        foreach (var existing in existingSchedules)
        {
            if (existing.GroupId != task.GroupId ||
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

        // Generated schedules.
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

        var generatedToday =
            CountGroupLessonsOnDay(
                task.GroupId,
                slot.DayOfWeek,
                generated);

        var existingToday =
            existingSchedules.Count(s =>
                s.GroupId == task.GroupId &&
                s.DayOfWeek == slot.DayOfWeek);

        if (generatedToday + existingToday >=
            request.MaxLessonsPerDay)
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

    private static int CountSubjectLessonsOnDay(
        int groupId,
        int subjectId,
        DayOfWeek day,
        List<GeneratedCandidate> generated)
    {
        return generated.Count(x =>
            x.GroupId == groupId &&
            x.SubjectId == subjectId &&
            x.DayOfWeek == day);
    }

    private static int CountGroupLessonsBeforeSlot(
        int groupId,
        GenerationSlot slot,
        List<GeneratedCandidate> generated)
    {
        return generated.Count(x =>
            x.GroupId == groupId &&
            x.DayOfWeek == slot.DayOfWeek &&
            x.SlotIndex < slot.SlotIndex);
    }

    // ============================================================
    // FINAL VALIDATION
    // ============================================================

    private static List<string> ValidateGeneratedSchedule(
        List<GeneratedCandidate> generated,
        List<LessonTask> lessonTasks,
        List<Group> groups,
        List<Teacher> teachers,
        List<Classroom> classrooms,
        List<Schedule> existingSchedules,
        ScheduleGenerationRequest request,
        List<GenerationTimeSlot> timeSlots)
    {
        var errors = new List<string>();

        // --------------------------------------------------------
        // COUNT
        // --------------------------------------------------------

        if (generated.Count != lessonTasks.Count)
        {
            errors.Add(
                $"Генератор создал {generated.Count} занятий " +
                $"из {lessonTasks.Count} необходимых.");

            return errors;
        }

        // --------------------------------------------------------
        // GROUP COURSE / SUBJECT COURSE
        // --------------------------------------------------------

        var groupById = groups.ToDictionary(
            g => g.Id);

        var subjectById = lessonTasks
            .Select(x => x.Subject)
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g => g.First());

        foreach (var item in generated)
        {
            if (!groupById.TryGetValue(
                    item.GroupId,
                    out var group))
            {
                errors.Add(
                    $"Группа для занятия «{item.SubjectName}» не найдена.");

                continue;
            }

            if (!subjectById.TryGetValue(
                    item.SubjectId,
                    out var subject))
            {
                errors.Add(
                    $"Предмет «{item.SubjectName}» не найден.");

                continue;
            }

            if (group.Course != subject.Course)
            {
                errors.Add(
                    $"Нарушено соответствие курса: " +
                    $"группа «{group.Name}» — {group.Course} курс, " +
                    $"предмет «{subject.Name}» — {subject.Course} курс.");
            }
        }

        // --------------------------------------------------------
        // SUBJECT LOAD
        // --------------------------------------------------------

        foreach (var group in groups)
        {
            var expected = lessonTasks
                .Where(x => x.GroupId == group.Id)
                .GroupBy(x => x.SubjectId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count());

            foreach (var pair in expected)
            {
                var actual = generated.Count(x =>
                    x.GroupId == group.Id &&
                    x.SubjectId == pair.Key);

                if (actual != pair.Value)
                {
                    var subjectName = lessonTasks
                        .First(x =>
                            x.GroupId == group.Id &&
                            x.SubjectId == pair.Key)
                        .SubjectName;

                    errors.Add(
                        $"Нагрузка по предмету «{subjectName}» " +
                        $"для группы «{group.Name}» " +
                        "сформирована неверно.");
                }
            }
        }

        // --------------------------------------------------------
        // GROUP CONFLICTS
        // --------------------------------------------------------

        foreach (var groupDay in generated
                     .GroupBy(x => new
                     {
                         x.GroupId,
                         x.DayOfWeek
                     }))
        {
            var items = groupDay.ToList();

            if (items.Count > request.MaxLessonsPerDay)
            {
                var groupName = items[0].GroupName;

                errors.Add(
                    $"Для группы «{groupName}» превышено " +
                    "максимальное количество занятий в день.");
            }

            for (var i = 0; i < items.Count; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    if (Overlaps(
                            items[i].StartTime,
                            items[i].EndTime,
                            items[j].StartTime,
                            items[j].EndTime))
                    {
                        errors.Add(
                            $"Обнаружен конфликт расписания " +
                            $"для группы «{items[i].GroupName}».");
                    }
                }
            }
        }

        // --------------------------------------------------------
        // TEACHER CONFLICTS
        // --------------------------------------------------------

        foreach (var teacherDay in generated
                     .GroupBy(x => new
                     {
                         x.TeacherId,
                         x.DayOfWeek
                     }))
        {
            var items = teacherDay.ToList();

            for (var i = 0; i < items.Count; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    if (Overlaps(
                            items[i].StartTime,
                            items[i].EndTime,
                            items[j].StartTime,
                            items[j].EndTime))
                    {
                        errors.Add(
                            $"Обнаружен конфликт расписания " +
                            $"для преподавателя «{items[i].TeacherName}».");
                    }
                }
            }
        }

        // --------------------------------------------------------
        // CLASSROOM CONFLICTS
        // --------------------------------------------------------

        foreach (var classroomDay in generated
                     .GroupBy(x => new
                     {
                         x.ClassroomId,
                         x.DayOfWeek
                     }))
        {
            var items = classroomDay.ToList();

            for (var i = 0; i < items.Count; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    if (Overlaps(
                            items[i].StartTime,
                            items[i].EndTime,
                            items[j].StartTime,
                            items[j].EndTime))
                    {
                        errors.Add(
                            $"Обнаружен конфликт аудитории " +
                            $"«{items[i].ClassroomName}».");
                    }
                }
            }
        }

        // --------------------------------------------------------
        // EXISTING SCHEDULE CONFLICTS
        // --------------------------------------------------------

        foreach (var item in generated)
        {
            foreach (var existing in existingSchedules)
            {
                if (existing.DayOfWeek != item.DayOfWeek ||
                    !existing.StartTime.HasValue ||
                    !existing.EndTime.HasValue)
                {
                    continue;
                }

                if (!Overlaps(
                        existing.StartTime.Value,
                        existing.EndTime.Value,
                        item.StartTime,
                        item.EndTime))
                {
                    continue;
                }

                if (existing.GroupId == item.GroupId)
                {
                    errors.Add(
                        $"Группа «{item.GroupName}» уже имеет " +
                        "занятие в выбранное время.");
                }

                if (existing.TeacherId == item.TeacherId)
                {
                    errors.Add(
                        $"Преподаватель «{item.TeacherName}» " +
                        "уже занят в выбранное время.");
                }

                if (existing.ClassroomId == item.ClassroomId)
                {
                    errors.Add(
                        $"Аудитория «{item.ClassroomName}» " +
                        "уже занята в выбранное время.");
                }
            }
        }

        // --------------------------------------------------------
        // TEACHER VALIDITY
        // --------------------------------------------------------
        //
        // Один преподаватель может вести несколько предметов.
        // Проверяем наличие конкретной пары TeacherId + SubjectId.
        // --------------------------------------------------------

        foreach (var item in generated)
        {
            var teacher = teachers.FirstOrDefault(
                t => t.Id == item.TeacherId);

            if (teacher == null)
            {
                errors.Add(
                    $"Преподаватель «{item.TeacherName}» не найден.");

                continue;
            }

            var canTeachSubject = teacher.TeacherSubjects.Any(
                ts => ts.SubjectId == item.SubjectId);

            if (!canTeachSubject)
            {
                errors.Add(
                    $"Преподаватель «{item.TeacherName}» " +
                    $"не может вести предмет «{item.SubjectName}».");
            }
        }

        // --------------------------------------------------------
        // CLASSROOM VALIDITY
        // --------------------------------------------------------

        var classroomById = classrooms
            .ToDictionary(c => c.Id);

        foreach (var item in generated)
        {
            if (!classroomById.TryGetValue(
                    item.ClassroomId,
                    out var classroom))
            {
                errors.Add(
                    $"Аудитория «{item.ClassroomName}» не найдена.");

                continue;
            }

            var group = groups.First(
                g => g.Id == item.GroupId);

            if (classroom.Capacity < group.StudentCount)
            {
                errors.Add(
                    $"Аудитория «{classroom.Name}» " +
                    $"не вмещает группу «{group.Name}».");
            }

            var task = lessonTasks.First(
                x =>
                    x.GroupId == item.GroupId &&
                    x.SubjectId == item.SubjectId);

            var requiredCategories = task.Subject
                .ClassroomCategoryRequirements
                .Select(x => x.ClassroomCategoryId)
                .Distinct()
                .ToHashSet();

            if (requiredCategories.Count > 0 &&
                !requiredCategories.Contains(
                    classroom.ClassroomCategoryId))
            {
                errors.Add(
                    $"Аудитория «{classroom.Name}» " +
                    "не соответствует требованиям " +
                    $"предмета «{item.SubjectName}».");
            }
        }

        // --------------------------------------------------------
        // CONSECUTIVE LESSONS
        // --------------------------------------------------------

        ValidateGeneratedConsecutiveLessons(
            generated,
            existingSchedules,
            timeSlots,
            request.MaxConsecutiveLessons,
            errors);

        return errors
            .Distinct()
            .ToList();
    }

    // ============================================================
    // PREVIEW INTERNAL CONFLICTS
    // ============================================================

    private static void ValidatePreviewInternalConflicts(
        List<GeneratedScheduleItem> items,
        List<string> errors)
    {
        ValidatePreviewEntityConflicts(
            items,
            x => x.GroupId,
            x => x.GroupName,
            "группы",
            "группе",
            errors);

        ValidatePreviewEntityConflicts(
            items,
            x => x.TeacherId,
            x => x.TeacherName,
            "преподавателя",
            "преподавателю",
            errors);

        ValidatePreviewEntityConflicts(
            items,
            x => x.ClassroomId,
            x => x.ClassroomName,
            "аудитории",
            "аудитории",
            errors);
    }

    private static void ValidatePreviewEntityConflicts<TKey>(
        List<GeneratedScheduleItem> items,
        Func<GeneratedScheduleItem, TKey> keySelector,
        Func<GeneratedScheduleItem, string> nameSelector,
        string entityName,
        string entityNameDative,
        List<string> errors)
    {
        foreach (var group in items.GroupBy(x => new
        {
            Key = keySelector(x),
            x.DayOfWeek
        }))
        {
            var dayItems = group.ToList();

            for (var i = 0; i < dayItems.Count; i++)
            {
                for (var j = i + 1; j < dayItems.Count; j++)
                {
                    if (!Overlaps(
                            dayItems[i].StartTime,
                            dayItems[i].EndTime,
                            dayItems[j].StartTime,
                            dayItems[j].EndTime))
                    {
                        continue;
                    }

                    errors.Add(
                        $"Обнаружен конфликт для {entityNameDative} " +
                        $"«{nameSelector(dayItems[i])}» " +
                        $"на {GetDayName(dayItems[i].DayOfWeek)}.");

                    break;
                }
            }
        }
    }

    // ============================================================
    // PREVIEW CONSECUTIVE LESSONS
    // ============================================================

    private static void ValidatePreviewConsecutiveLessons(
        List<GeneratedScheduleItem> previewItems,
        List<Schedule> protectedSchedules,
        List<GenerationTimeSlot> timeSlots,
        int maxConsecutiveLessons,
        List<string> errors)
    {
        if (maxConsecutiveLessons <= 0 ||
            timeSlots.Count == 0)
        {
            return;
        }

        foreach (var groupDay in previewItems
                     .GroupBy(x => new
                     {
                         x.GroupId,
                         x.DayOfWeek
                     }))
        {
            var groupItems = groupDay.ToList();

            for (var slotIndex = 0;
                 slotIndex < timeSlots.Count;
                 slotIndex++)
            {
                var slot = timeSlots[slotIndex];

                if (!groupItems.Any(item =>
                        Overlaps(
                            item.StartTime,
                            item.EndTime,
                            slot.StartTime,
                            slot.EndTime)))
                {
                    continue;
                }

                var consecutive = CountConsecutivePreviewSlots(
                    groupDay.Key.GroupId,
                    groupDay.Key.DayOfWeek,
                    slotIndex,
                    groupItems,
                    protectedSchedules,
                    timeSlots);

                if (consecutive > maxConsecutiveLessons)
                {
                    var groupName = groupItems[0].GroupName;

                    errors.Add(
                        $"Для группы «{groupName}» на " +
                        $"{GetDayName(groupDay.Key.DayOfWeek)} " +
                        $"получилось {consecutive} занятий подряд. " +
                        $"Допустимо максимум {maxConsecutiveLessons}.");

                    break;
                }
            }
        }
    }

    private static int CountConsecutivePreviewSlots(
        int groupId,
        DayOfWeek day,
        int targetSlotIndex,
        List<GeneratedScheduleItem> previewItems,
        List<Schedule> protectedSchedules,
        List<GenerationTimeSlot> timeSlots)
    {
        var occupied = new HashSet<int>();

        for (var i = 0; i < timeSlots.Count; i++)
        {
            var slot = timeSlots[i];

            var occupiedByPreview = previewItems.Any(item =>
                Overlaps(
                    item.StartTime,
                    item.EndTime,
                    slot.StartTime,
                    slot.EndTime));

            var occupiedByExisting = protectedSchedules.Any(existing =>
                existing.GroupId == groupId &&
                existing.DayOfWeek == day &&
                existing.StartTime.HasValue &&
                existing.EndTime.HasValue &&
                Overlaps(
                    existing.StartTime.Value,
                    existing.EndTime.Value,
                    slot.StartTime,
                    slot.EndTime));

            if (occupiedByPreview || occupiedByExisting)
            {
                occupied.Add(i);
            }
        }

        if (!occupied.Contains(targetSlotIndex))
        {
            return 0;
        }

        var count = 1;

        var left = targetSlotIndex - 1;

        while (occupied.Contains(left))
        {
            count++;
            left--;
        }

        var right = targetSlotIndex + 1;

        while (occupied.Contains(right))
        {
            count++;
            right++;
        }

        return count;
    }

    // ============================================================
    // GENERATED CONSECUTIVE LESSONS
    // ============================================================

    private static void ValidateGeneratedConsecutiveLessons(
        List<GeneratedCandidate> generated,
        List<Schedule> existingSchedules,
        List<GenerationTimeSlot> timeSlots,
        int maxConsecutiveLessons,
        List<string> errors)
    {
        if (maxConsecutiveLessons <= 0 ||
            timeSlots.Count == 0)
        {
            return;
        }

        foreach (var groupDay in generated
                     .GroupBy(x => new
                     {
                         x.GroupId,
                         x.DayOfWeek
                     }))
        {
            var generatedItems = groupDay.ToList();

            for (var slotIndex = 0;
                 slotIndex < timeSlots.Count;
                 slotIndex++)
            {
                var slot = timeSlots[slotIndex];

                if (!generatedItems.Any(item =>
                        Overlaps(
                            item.StartTime,
                            item.EndTime,
                            slot.StartTime,
                            slot.EndTime)))
                {
                    continue;
                }

                var occupied = new HashSet<int>();

                for (var i = 0; i < timeSlots.Count; i++)
                {
                    var currentSlot = timeSlots[i];

                    var generatedOccupied =
                        generatedItems.Any(item =>
                            Overlaps(
                                item.StartTime,
                                item.EndTime,
                                currentSlot.StartTime,
                                currentSlot.EndTime));

                    var existingOccupied =
                        existingSchedules.Any(existing =>
                            existing.GroupId == groupDay.Key.GroupId &&
                            existing.DayOfWeek == groupDay.Key.DayOfWeek &&
                            existing.StartTime.HasValue &&
                            existing.EndTime.HasValue &&
                            Overlaps(
                                existing.StartTime.Value,
                                existing.EndTime.Value,
                                currentSlot.StartTime,
                                currentSlot.EndTime));

                    if (generatedOccupied || existingOccupied)
                    {
                        occupied.Add(i);
                    }
                }

                if (!occupied.Contains(slotIndex))
                {
                    continue;
                }

                var consecutive = 1;

                var left = slotIndex - 1;

                while (occupied.Contains(left))
                {
                    consecutive++;
                    left--;
                }

                var right = slotIndex + 1;

                while (occupied.Contains(right))
                {
                    consecutive++;
                    right++;
                }

                if (consecutive > maxConsecutiveLessons)
                {
                    errors.Add(
                        $"Для группы «{generatedItems[0].GroupName}» " +
                        $"на {GetDayName(groupDay.Key.DayOfWeek)} " +
                        $"получилось {consecutive} занятий подряд. " +
                        $"Допустимо максимум {maxConsecutiveLessons}.");

                    break;
                }
            }
        }
    }

    // ============================================================
    // DAY NAME
    // ============================================================

    private static string GetDayName(
        DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "понедельник",
            DayOfWeek.Tuesday => "вторник",
            DayOfWeek.Wednesday => "среду",
            DayOfWeek.Thursday => "четверг",
            DayOfWeek.Friday => "пятницу",
            DayOfWeek.Saturday => "субботу",
            DayOfWeek.Sunday => "воскресенье",
            _ => day.ToString()
        };
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
    }
}