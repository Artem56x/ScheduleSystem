using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Services;

public class TeacherRecommendationService
{
    private readonly ApplicationDbContext _context;

    public TeacherRecommendationService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // RECOMMENDATIONS
    // =========================================================

    public async Task<List<Teacher>> GetRecommendationsAsync(
        Schedule schedule,
        Subject subject,
        Group group)
    {
        // =====================================================
        // AVAILABLE TEACHERS FOR SUBJECT
        // =====================================================

        var teachers = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .Where(t =>
                t.Id != schedule.TeacherId &&
                t.TeacherSubjects.Any(ts =>
                    ts.SubjectId == subject.Id))
            .OrderBy(t => t.FullName)
            .ToListAsync();

        if (teachers.Count == 0)
        {
            return new List<Teacher>();
        }

        // =====================================================
        // BUSY TEACHERS
        // =====================================================

        var teacherIds = teachers
            .Select(t => t.Id)
            .ToList();

        var busyTeacherIds = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.Id != schedule.Id &&
                teacherIds.Contains(s.TeacherId) &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime)
            .Select(s => s.TeacherId)
            .Distinct()
            .ToListAsync();

        var busyIds = busyTeacherIds.ToHashSet();

        // =====================================================
        // RETURN FREE TEACHERS
        // =====================================================

        return teachers
            .Where(t => !busyIds.Contains(t.Id))
            .ToList();
    }

    // =========================================================
    // SELECTED TEACHER
    // =========================================================

    public async Task<Teacher?> GetSelectedTeacherAsync(
        int teacherId)
    {
        if (teacherId <= 0)
        {
            return null;
        }

        return await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(
                t => t.Id == teacherId);
    }

    // =========================================================
    // TEACHER PROBLEM
    // =========================================================

    public async Task<string?> GetTeacherProblemAsync(
        Schedule schedule)
    {
        if (schedule.TeacherId <= 0)
        {
            return null;
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(
                t => t.Id == schedule.TeacherId);

        if (teacher == null)
        {
            return null;
        }

        var conflict = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Subject)
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s =>
                s.Id != schedule.Id &&
                s.TeacherId == schedule.TeacherId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);

        if (conflict == null)
        {
            return null;
        }

        var subjectName =
            conflict.Subject?.Name
            ?? "Неизвестный предмет";

        var groupName =
            conflict.Group?.Name
            ?? "Неизвестная группа";

        var time =
            FormatTimeRange(
                conflict.StartTime,
                conflict.EndTime);

        return
            $"преподаватель занят: «{subjectName}» — " +
            $"группа «{groupName}» — {time}";
    }

    // =========================================================
    // TIME FORMAT
    // =========================================================

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

