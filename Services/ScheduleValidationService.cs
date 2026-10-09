using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Services;

public class ScheduleValidationService
{
    private readonly ApplicationDbContext _context;

    public ScheduleValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleValidationResult> ValidateAsync(
        Schedule schedule,
        bool confirmTeacherSubject)
    {
        var result = new ScheduleValidationResult();

        // =========================================================
        // BASIC VALIDATION
        // =========================================================


        // =========================================================
        // BASIC VALIDATION
        // =========================================================

        if (!schedule.DayOfWeek.HasValue)
        {
            result.AddError(
                "DayOfWeek",
                "Выберите день недели.");
        }
        else if (!Enum.IsDefined(
                     typeof(DayOfWeek),
                     schedule.DayOfWeek.Value))
        {
            result.AddError(
                "DayOfWeek",
                "Выбран некорректный день недели.");
        }

        if (!schedule.StartTime.HasValue ||
            !schedule.EndTime.HasValue)
        {
            result.AddError(
                "",
                "Укажите время начала и окончания занятия.");

            return result;
        }

        var startTime = schedule.StartTime.Value;
        var endTime = schedule.EndTime.Value;
        var dayLength = TimeSpan.FromDays(1);

        if (startTime < TimeSpan.Zero ||
            startTime >= dayLength)
        {
            result.AddError(
                "StartTime",
                "Время начала должно быть в пределах суток.");
        }

        if (endTime < TimeSpan.Zero ||
            endTime >= dayLength)
        {
            result.AddError(
                "EndTime",
                "Время окончания должно быть в пределах суток.");
        }

        if (startTime >= TimeSpan.Zero &&
            startTime < dayLength &&
            endTime >= TimeSpan.Zero &&
            endTime < dayLength &&
            startTime >= endTime)
        {
            result.AddError(
                "EndTime",
                "Время окончания должно быть позже времени начала.");
        }

        if (!result.IsValid)
        {
            return result;
        }

        // =========================================================
        // RELATED ENTITIES
        // =========================================================

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(
                t => t.Id == schedule.TeacherId);

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.Id == schedule.GroupId);

        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
                .ThenInclude(x => x.ClassroomCategory)
            .FirstOrDefaultAsync(
                s => s.Id == schedule.SubjectId);

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(
                c => c.Id == schedule.ClassroomId);

        if (teacher == null)
        {
            result.AddError(
                "TeacherId",
                "Преподаватель не найден.");
        }

        if (group == null)
        {
            result.AddError(
                "GroupId",
                "Группа не найдена.");
        }

        if (subject == null)
        {
            result.AddError(
                "SubjectId",
                "Предмет не найден.");
        }

        if (classroom == null)
        {
            result.AddError(
                "ClassroomId",
                "Аудитория не найдена.");
        }

        if (!result.IsValid)
        {
            return result;
        }

        // =========================================================
        // COURSE CHECK
        // =========================================================

        if (group!.Course != subject!.Course)
        {
            result.AddError(
                "SubjectId",
                $"Предмет «{subject.Name}» относится к {subject.Course} курсу, " +
                $"а группа «{group.Name}» обучается на {group.Course} курсе. " +
                "Выберите предмет своего курса.");

            return result;
        }

        // =========================================================
        // TEACHER / SUBJECT WARNING
        // =========================================================

        var teachesSelectedSubject = teacher!.TeacherSubjects
            .Any(ts => ts.SubjectId == subject!.Id);

        if (!teachesSelectedSubject)
        {
            result.TeacherSubjectWarning = true;
            result.TeacherName = teacher.FullName;

            result.TeacherSubjectName =
                teacher.TeacherSubjects
                    .Select(ts => ts.Subject?.Name)
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList() is var teacherSubjectNames &&
                teacherSubjectNames.Count > 0
                    ? string.Join(", ", teacherSubjectNames)
                    : "не назначены";

            result.SelectedSubjectName =
                subject.Name;

            if (!confirmTeacherSubject)
            {
                result.AddError(
                    "",
                    $"Преподаватель «{teacher.FullName}» не закреплён " +
                    $"за предметом «{subject.Name}». Проверьте выбор.");
            }
        }

        // =========================================================
        // CAPACITY
        // =========================================================

        if (group.StudentCount > classroom!.Capacity)
        {
            result.AddError(
                "ClassroomId",
                $"В аудитории «{classroom.Name}» недостаточно мест. " +
                $"Вместимость: {classroom.Capacity}, " +
                $"в группе: {group.StudentCount} человек.");

            result.ClassroomRecommendationNeeded = true;
        }

        // =========================================================
        // CLASSROOM CATEGORY REQUIREMENTS
        // =========================================================

        var requiredCategoryIds = subject!
            .ClassroomCategoryRequirements
            .Where(x => x.ClassroomCategory != null)
            .Select(x => x.ClassroomCategoryId)
            .Distinct()
            .ToList();

        if (requiredCategoryIds.Count > 0)
        {
            var classroomCategoryId =
                classroom!.ClassroomCategoryId;

            if (!requiredCategoryIds.Contains(
                    classroomCategoryId))
            {
                var requiredCategoryNames = subject
                    .ClassroomCategoryRequirements
                    .Where(x =>
                        x.ClassroomCategory != null)
                    .Select(x =>
                        x.ClassroomCategory.Name)
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();

                var categoryText =
                    requiredCategoryNames.Count == 1
                        ? $"«{requiredCategoryNames[0]}»"
                        : string.Join(
                            ", ",
                            requiredCategoryNames.Select(
                                name => $"«{name}»"));

                result.AddError(
                    "ClassroomId",
                    $"Для предмета «{subject.Name}» требуется " +
                    $"аудитория категории {categoryText}. " +
                    $"Выбрана категория «{classroom!.ClassroomCategory?.Name ?? "не указана"}».");

                result.ClassroomRecommendationNeeded = true;
            }
        }

        // =========================================================
        // CLASSROOM CONFLICT
        // =========================================================

        var classroomConflict =
            await FindClassroomConflictAsync(schedule);

        if (classroomConflict != null)
        {
            result.AddError(
                "ClassroomId",
                $"Аудитория «{classroom!.Name}» уже занята: " +
                $"«{classroomConflict.Subject?.Name ?? "Неизвестный предмет"}» — " +
                $"группа «{classroomConflict.Group?.Name ?? "Неизвестная группа"}» — " +
                $"{FormatTimeRange(
                    classroomConflict.StartTime,
                    classroomConflict.EndTime)}.");

            result.ClassroomRecommendationNeeded = true;
        }

        // =========================================================
        // GROUP CONFLICT
        // =========================================================

        var groupConflict =
            await FindGroupConflictAsync(schedule);

        if (groupConflict != null)
        {
            result.AddError(
                "GroupId",
                $"У группы «{group!.Name}» уже есть занятие: " +
                $"«{groupConflict.Subject?.Name ?? "Неизвестный предмет"}» — " +
                $"преподаватель «{groupConflict.Teacher?.FullName ?? "Неизвестный преподаватель"}» — " +
                $"{FormatTimeRange(
                    groupConflict.StartTime,
                    groupConflict.EndTime)}.");
        }

        // =========================================================
        // TEACHER CONFLICT
        // =========================================================

        var teacherConflict =
            await FindTeacherConflictAsync(schedule);

        if (teacherConflict != null)
        {
            result.TeacherConflict = true;

            result.AddError(
                "TeacherId",
                $"У преподавателя «{teacher!.FullName}» уже есть занятие: " +
                $"«{teacherConflict.Subject?.Name ?? "Неизвестный предмет"}» — " +
                $"группа «{teacherConflict.Group?.Name ?? "Неизвестная группа"}» — " +
                $"{FormatTimeRange(
                    teacherConflict.StartTime,
                    teacherConflict.EndTime)}.");
        }

        return result;
    }

    // =============================================================
    // CONFLICT SEARCH
    // =============================================================

    private async Task<Schedule?> FindClassroomConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Subject)
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s =>
                s.Id != schedule.Id &&
                s.ClassroomId == schedule.ClassroomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }

    private async Task<Schedule?> FindGroupConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s =>
                s.Id != schedule.Id &&
                s.GroupId == schedule.GroupId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }

    private async Task<Schedule?> FindTeacherConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Subject)
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s =>
                s.Id != schedule.Id &&
                s.TeacherId == schedule.TeacherId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }

    // =============================================================
    // HELPERS
    // =============================================================

    private static string FormatTimeRange(
        TimeSpan? startTime,
        TimeSpan? endTime)
    {
        var start = startTime.HasValue
            ? startTime.Value.ToString(@"hh\:mm")
            : "—";

        var end = endTime.HasValue
            ? endTime.Value.ToString(@"hh\:mm")
            : "—";

        return $"{start}–{end}";
    }
}

// =============================================================
// VALIDATION RESULT
// =============================================================

public class ScheduleValidationResult
{
    private readonly List<ScheduleValidationError> _errors = new();

    public IReadOnlyList<ScheduleValidationError> Errors =>
        _errors;

    public bool IsValid =>
        _errors.Count == 0;

    public bool TeacherSubjectWarning { get; set; }

    public bool TeacherConflict { get; set; }

    public string? TeacherName { get; set; }

    public string? TeacherSubjectName { get; set; }

    public string? SelectedSubjectName { get; set; }

    public bool ClassroomRecommendationNeeded { get; set; }

    public void AddError(
        string key,
        string message)
    {
        _errors.Add(
            new ScheduleValidationError(
                key,
                message));
    }
}

// =============================================================
// VALIDATION ERROR
// =============================================================

public class ScheduleValidationError
{
    public string Key { get; }

    public string Message { get; }

    public ScheduleValidationError(
        string key,
        string message)
    {
        Key = key;
        Message = message;
    }
}

