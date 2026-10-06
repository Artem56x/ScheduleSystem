using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Services;

public class ClassroomRecommendationService
{
    private readonly ApplicationDbContext _context;

    public ClassroomRecommendationService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // RECOMMENDATIONS
    // =========================================================

    public async Task<List<Classroom>> GetRecommendationsAsync(
        Schedule schedule,
        Subject subject,
        Group group)
    {
        var requiredCategoryIds = subject
            .ClassroomCategoryRequirements
            .Select(x => x.ClassroomCategoryId)
            .Distinct()
            .ToList();

        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .Where(c =>
                c.Id != schedule.ClassroomId &&
                c.Capacity >= group.StudentCount)
            .OrderBy(c => c.Capacity)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // =====================================================
        // CATEGORY REQUIREMENTS
        // =====================================================

        if (requiredCategoryIds.Count > 0)
        {
            classrooms = classrooms
                .Where(c =>
                    requiredCategoryIds.Contains(
                        c.ClassroomCategoryId))
                .ToList();
        }

        if (classrooms.Count == 0)
        {
            return new List<Classroom>();
        }

        // =====================================================
        // BUSY CLASSROOMS
        // =====================================================

        var classroomIds = classrooms
            .Select(c => c.Id)
            .ToList();

        var busyClassroomIds = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.Id != schedule.Id &&
                classroomIds.Contains(s.ClassroomId) &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime)
            .Select(s => s.ClassroomId)
            .Distinct()
            .ToListAsync();

        var busyIds = busyClassroomIds.ToHashSet();

        // =====================================================
        // RETURN FREE CLASSROOMS
        // =====================================================

        return classrooms
            .Where(c => !busyIds.Contains(c.Id))
            .ToList();
    }

    // =========================================================
    // SELECTED CLASSROOM NAME
    // =========================================================

    public async Task<string?> GetSelectedClassroomNameAsync(
        int classroomId)
    {
        if (classroomId <= 0)
        {
            return null;
        }

        return await _context.Classrooms
            .AsNoTracking()
            .Where(c => c.Id == classroomId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }

    // =========================================================
    // SELECTED CLASSROOM
    // =========================================================

    public async Task<Classroom?> GetSelectedClassroomAsync(
        int classroomId)
    {
        if (classroomId <= 0)
        {
            return null;
        }

        return await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(
                c => c.Id == classroomId);
    }

    // =========================================================
    // CLASSROOM REASON
    // =========================================================

    public async Task<string?> GetClassroomProblemAsync(
        Schedule schedule,
        Subject subject,
        Group group)
    {
        if (schedule.ClassroomId <= 0)
        {
            return null;
        }

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(
                c => c.Id == schedule.ClassroomId);

        if (classroom == null)
        {
            return null;
        }

        // =====================================================
        // CAPACITY
        // =====================================================

        if (classroom.Capacity < group.StudentCount)
        {
            return
                $"недостаточно мест: вместимость " +
                $"{classroom.Capacity}, в группе " +
                $"{group.StudentCount} человек";
        }

        // =====================================================
        // CATEGORY
        // =====================================================

        var requiredCategoryIds = subject
            .ClassroomCategoryRequirements
            .Select(x => x.ClassroomCategoryId)
            .Distinct()
            .ToList();

        if (requiredCategoryIds.Count > 0 &&
            !requiredCategoryIds.Contains(
                classroom.ClassroomCategoryId))
        {
            var requiredCategoryNames = subject
                .ClassroomCategoryRequirements
                .Where(x => x.ClassroomCategory != null)
                .Select(x => x.ClassroomCategory.Name)
                .Where(name =>
                    !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            var requiredCategoryText =
                requiredCategoryNames.Count == 1
                    ? $"«{requiredCategoryNames[0]}»"
                    : string.Join(
                        ", ",
                        requiredCategoryNames.Select(
                            name => $"«{name}»"));

            var selectedCategory =
                classroom.ClassroomCategory?.Name
                ?? "не указана";

            return
                $"неподходящая категория: требуется " +
                $"{requiredCategoryText}, выбрана " +
                $"«{selectedCategory}»";
        }

        // =====================================================
        // CLASSROOM CONFLICT
        // =====================================================

        var conflict = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Subject)
            .Include(s => s.Group)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s =>
                s.Id != schedule.Id &&
                s.ClassroomId == schedule.ClassroomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);

        if (conflict != null)
        {
            var subjectName =
                conflict.Subject?.Name
                ?? "Неизвестный предмет";

            var groupName =
                conflict.Group?.Name
                ?? "Неизвестная группа";

            var teacherName =
                conflict.Teacher?.FullName
                ?? "Неизвестный преподаватель";

            var time =
                FormatTimeRange(
                    conflict.StartTime,
                    conflict.EndTime);

            return
                $"аудитория занята: «{subjectName}» — " +
                $"группа «{groupName}» — " +
                $"преподаватель «{teacherName}» — " +
                $"{time}";
        }

        return null;
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