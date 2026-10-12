using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using System.Security.Claims;

namespace ScheduleSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: Home
    public async Task<IActionResult> Index()
    {
        // =========================================================
        // СТАТИСТИКА
        // =========================================================

        ViewBag.TeachersCount = await _context.Teachers
            .AsNoTracking()
            .CountAsync();

        ViewBag.GroupsCount = await _context.Groups
            .AsNoTracking()
            .CountAsync();

        ViewBag.SubjectsCount = await _context.Subjects
            .AsNoTracking()
            .CountAsync();

        ViewBag.ClassroomsCount = await _context.Classrooms
            .AsNoTracking()
            .CountAsync();



        // =========================================================
        // ПОСЛЕДНИЕ УВЕДОМЛЕНИЯ
        // =========================================================

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (User.Identity?.IsAuthenticated == true &&
            !string.IsNullOrEmpty(userId))
        {
            ViewBag.LatestNotifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .ToListAsync();
        }
        else
        {
            ViewBag.LatestNotifications = new List<Notification>();
        }




        // =========================================================
        // РАСПИСАНИЕ
        // =========================================================

        var schedule = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return View(schedule);
    }


    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }


    // GET: Home/Error
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId =
                Activity.Current?.Id ??
                HttpContext.TraceIdentifier
        });
    }
}

