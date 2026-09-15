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

    public async Task<List<Classroom>> GetRecommendationsAsync(
        Schedule schedule,
        Subject subject,
        Group group)
    {
        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .Where(c =>
                c.Id != schedule.ClassroomId &&
                c.Capacity >= group.StudentCount &&
                (!subject.RequiresComputers ||
                 c.HasComputers))
            .OrderBy(c => c.Capacity)
            .ThenBy(c => c.Name)
            .ToListAsync();

        if (classrooms.Count == 0)
        {
            return new List<Classroom>();
        }

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

        return classrooms
            .Where(c => !busyIds.Contains(c.Id))
            .ToList();
    }

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
}