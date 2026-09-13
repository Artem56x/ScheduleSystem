using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

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

    public async Task<IActionResult> Index()
    {
        ViewBag.TeachersCount = await _context.Teachers.CountAsync();
        ViewBag.GroupsCount = await _context.Groups.CountAsync();
        ViewBag.SubjectsCount = await _context.Subjects.CountAsync();
        ViewBag.ClassroomsCount = await _context.Classrooms.CountAsync();

        var schedule = await _context.Schedules
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return View(schedule);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}