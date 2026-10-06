using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.ViewModels;

namespace ScheduleSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // ============================================================
    // АНАЛИТИКА
    // ============================================================

    public async Task<IActionResult> Index()
    {
        var model = new AnalyticsViewModel();


        // ========================================================
        // ОБЩАЯ СТАТИСТИКА
        // ========================================================

        model.TeachersCount = await _context.Teachers.CountAsync();

        model.GroupsCount = await _context.Groups.CountAsync();

        model.SubjectsCount = await _context.Subjects.CountAsync();

        model.ClassroomsCount = await _context.Classrooms.CountAsync();

        model.SchedulesCount = await _context.Schedules.CountAsync();


        // ========================================================
        // ПОЛУЧАЕМ РАСПИСАНИЕ
        // ========================================================

        var schedules = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .ToListAsync();


        // ========================================================
        // НАЗВАНИЯ ДНЕЙ
        // ========================================================

        var dayNames = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "Понедельник" },
            { DayOfWeek.Tuesday, "Вторник" },
            { DayOfWeek.Wednesday, "Среда" },
            { DayOfWeek.Thursday, "Четверг" },
            { DayOfWeek.Friday, "Пятница" },
            { DayOfWeek.Saturday, "Суббота" },
            { DayOfWeek.Sunday, "Воскресенье" }
        };


        var orderedDays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };


        // ========================================================
        // ЗАНЯТИЯ ПО ДНЯМ
        // ========================================================

        model.LessonsByDay = orderedDays
            .Select(day => new DayAnalyticsItem
            {
                DayName = dayNames[day],
                Count = schedules.Count(s => s.DayOfWeek == day)
            })
            .ToList();


        // ========================================================
        // САМЫЙ ЗАГРУЖЕННЫЙ ДЕНЬ
        // ========================================================

        var busiestDay = model.LessonsByDay
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        if (busiestDay != null && busiestDay.Count > 0)
        {
            model.BusiestDay = busiestDay.DayName;
            model.BusiestDayCount = busiestDay.Count;
        }


        // ========================================================
        // ЗАНЯТИЯ ПО ВРЕМЕНИ НАЧАЛА
        // ========================================================

        model.LessonsByTime = schedules
            .Where(s => s.StartTime.HasValue)
            .GroupBy(s => s.StartTime!.Value)
            .OrderBy(g => g.Key)
            .Select(g => new TimeAnalyticsItem
            {
                Time = g.Key.ToString(@"hh\:mm"),
                Count = g.Count()
            })
            .ToList();

        // ========================================================
        // САМОЕ ЗАГРУЖЕННОЕ ВРЕМЯ
        // ========================================================

        var busiestTime = model.LessonsByTime
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        if (busiestTime != null && busiestTime.Count > 0)
        {
            model.BusiestTime = busiestTime.Time;
            model.BusiestTimeCount = busiestTime.Count;
        }


        // ========================================================
        // НАГРУЗКА ПРЕПОДАВАТЕЛЕЙ
        // ========================================================

        model.TeacherLoad = schedules
            .Where(s => s.Teacher != null)
            .GroupBy(s => new
            {
                s.TeacherId,
                Name = s.Teacher!.FullName
            })
            .Select(g => new TeacherAnalyticsItem
            {
                Id = g.Key.TeacherId,
                Name = g.Key.Name,
                LessonsCount = g.Count()
            })
            .OrderByDescending(x => x.LessonsCount)
            .ThenBy(x => x.Name)
            .ToList();


        // ========================================================
        // НАГРУЗКА ГРУПП
        // ========================================================

        model.GroupLoad = schedules
            .Where(s => s.Group != null)
            .GroupBy(s => new
            {
                s.GroupId,
                Name = s.Group!.Name
            })
            .Select(g => new GroupAnalyticsItem
            {
                Id = g.Key.GroupId,
                Name = g.Key.Name,
                LessonsCount = g.Count()
            })
            .OrderByDescending(x => x.LessonsCount)
            .ThenBy(x => x.Name)
            .ToList();


        // ========================================================
        // ИСПОЛЬЗОВАНИЕ АУДИТОРИЙ
        // ========================================================

        model.ClassroomUsage = schedules
            .Where(s => s.Classroom != null)
            .GroupBy(s => new
            {
                s.ClassroomId,
                Name = s.Classroom!.Name,
                Capacity = s.Classroom.Capacity
            })
            .Select(g => new ClassroomAnalyticsItem
            {
                Id = g.Key.ClassroomId,
                Name = g.Key.Name,
                LessonsCount = g.Count(),
                Capacity = g.Key.Capacity
            })
            .OrderByDescending(x => x.LessonsCount)
            .ThenBy(x => x.Name)
            .ToList();


        // ========================================================
        // ИСПОЛЬЗОВАНИЕ ПРЕДМЕТОВ
        // ========================================================

        model.SubjectUsage = schedules
            .Where(s => s.Subject != null)
            .GroupBy(s => new
            {
                s.SubjectId,
                Name = s.Subject!.Name
            })
            .Select(g => new SubjectAnalyticsItem
            {
                Id = g.Key.SubjectId,
                Name = g.Key.Name,
                LessonsCount = g.Count()
            })
            .OrderByDescending(x => x.LessonsCount)
            .ThenBy(x => x.Name)
            .ToList();


        // ========================================================
        // СРЕДНЕЕ КОЛИЧЕСТВО ЗАНЯТИЙ В ДЕНЬ
        // ========================================================

        model.AverageLessonsPerDay = model.SchedulesCount / 7.0;


        // ========================================================
        // СРЕДНЕЕ КОЛИЧЕСТВО ЗАНЯТИЙ НА ПРЕПОДАВАТЕЛЯ
        // ========================================================

        if (model.TeachersCount > 0)
        {
            model.AverageLessonsPerTeacher =
                (double)model.SchedulesCount / model.TeachersCount;
        }


        // ========================================================
        // СРЕДНЕЕ КОЛИЧЕСТВО ЗАНЯТИЙ НА ГРУППУ
        // ========================================================

        if (model.GroupsCount > 0)
        {
            model.AverageLessonsPerGroup =
                (double)model.SchedulesCount / model.GroupsCount;
        }


        // ========================================================
        // ПЕРЕДАЁМ МОДЕЛЬ В VIEW
        // ========================================================

        return View(model);
    }
}