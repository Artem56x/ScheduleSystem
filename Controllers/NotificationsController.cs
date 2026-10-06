using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;

namespace ScheduleSystem.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // СПИСОК УВЕДОМЛЕНИЙ
    // ============================================================

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(notifications);
    }


    // ============================================================
    // ПРОЧИТАТЬ ОДНО УВЕДОМЛЕНИЕ
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // ПРОЧИТАТЬ ВСЕ
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var notifications = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}