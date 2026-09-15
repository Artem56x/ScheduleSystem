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

        // --------------------------------------------------------
        // Базовая проверка
        // --------------------------------------------------------

        if (!schedule.DayOfWeek.HasValue)
        {
            result.AddError(
                "DayOfWeek",
                "Выберите день недели.");
        }

        if (!schedule.StartTime.HasValue ||
            !schedule.EndTime.HasValue)
        {
            result.AddError(
                "",
                "Укажите время начала и окончания занятия.");

            return result;
        }

        if (schedule.StartTime >= schedule.EndTime)
        {
            result.AddError(
                "",
                "Время окончания должно быть позже времени начала.");

            return result;
        }

        // --------------------------------------------------------
        // Проверяем связанные сущности
        // --------------------------------------------------------

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == schedule.TeacherId);

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.Id == schedule.GroupId);

        var subject = await _context.Subjects
            .AsNoTracking()
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

        // --------------------------------------------------------
        // Преподаватель / предмет
        // --------------------------------------------------------

        if (teacher!.SubjectId.HasValue &&
            teacher.SubjectId.Value != subject!.Id)
        {
            var teacherSubject = await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.Id == teacher.SubjectId.Value);

            result.TeacherSubjectWarning = true;
            result.TeacherName = teacher.FullName;

            result.TeacherSubjectName =
                teacherSubject?.Name ?? "неизвестный предмет";

            result.SelectedSubjectName =
                subject.Name;

            if (!confirmTeacherSubject)
            {
                result.AddError(
                    "",
                    $"Преподаватель «{teacher.FullName}» обычно ведёт " +
                    $"другой предмет. Проверьте выбор.");
            }
        }

        // --------------------------------------------------------
        // Вместимость группы / аудитории
        // --------------------------------------------------------

        if (group!.StudentCount > classroom!.Capacity)
        {
            result.AddError(
                "ClassroomId",
                $"В аудитории «{classroom.Name}» недостаточно мест. " +
                $"Вместимость: {classroom.Capacity}, " +
                $"в группе: {group.StudentCount} человек.");

            result.ClassroomRecommendationNeeded = true;
        }

        // --------------------------------------------------------
        // Требование компьютеров
        // --------------------------------------------------------

        if (subject!.RequiresComputers &&
            !classroom.HasComputers)
        {
            result.AddError(
                "ClassroomId",
                $"Предмет «{subject.Name}» требует компьютерную " +
                $"аудиторию. В аудитории «{classroom.Name}» " +
                $"компьютеров нет.");

            result.ClassroomRecommendationNeeded = true;
        }

        // --------------------------------------------------------
        // Конфликт аудитории
        // --------------------------------------------------------

        if (await HasClassroomConflictAsync(schedule))
        {
            result.AddError(
                "ClassroomId",
                $"Аудитория «{classroom.Name}» уже занята " +
                $"в выбранное время.");

            result.ClassroomRecommendationNeeded = true;
        }

        // --------------------------------------------------------
        // Конфликт группы
        // --------------------------------------------------------

        if (await HasGroupConflictAsync(schedule))
        {
            result.AddError(
                "GroupId",
                $"У группы «{group.Name}» уже есть занятие " +
                $"в выбранное время.");
        }

        // --------------------------------------------------------
        // Конфликт преподавателя
        // --------------------------------------------------------

        if (await HasTeacherConflictAsync(schedule))
        {
            result.AddError(
                "TeacherId",
                $"У преподавателя «{teacher.FullName}» уже есть " +
                $"занятие в выбранное время.");
        }

        return result;
    }

    // ============================================================
    // CONFLICTS
    // ============================================================

    private async Task<bool> HasClassroomConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.ClassroomId == schedule.ClassroomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }

    private async Task<bool> HasGroupConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.GroupId == schedule.GroupId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }

    private async Task<bool> HasTeacherConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.TeacherId == schedule.TeacherId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }
}


// ============================================================
// VALIDATION RESULT
// ============================================================

public class ScheduleValidationResult
{
    private readonly List<ScheduleValidationError> _errors = new();

    public IReadOnlyList<ScheduleValidationError> Errors =>
        _errors;

    public bool IsValid =>
        _errors.Count == 0;

    public bool TeacherSubjectWarning { get; set; }

    public string? TeacherName { get; set; }

    public string? TeacherSubjectName { get; set; }

    public string? SelectedSubjectName { get; set; }

    public bool ClassroomRecommendationNeeded { get; set; }

    public void AddError(
        string key,
        string message)
    {
        _errors.Add(
            new ScheduleValidationError(key, message));
    }
}


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