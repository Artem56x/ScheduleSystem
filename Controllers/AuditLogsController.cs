using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;

namespace ScheduleSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Include(log => log.User)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();

        return View(logs);
    }
}