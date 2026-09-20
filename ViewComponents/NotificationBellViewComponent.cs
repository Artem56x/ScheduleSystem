using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;

namespace ScheduleSystem.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationBellViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return View(0);
        }

        var claimsPrincipal = User as ClaimsPrincipal;

        if (claimsPrincipal == null)
        {
            return View(0);
        }

        var userId = claimsPrincipal.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrEmpty(userId))
        {
            return View(0);
        }

        var unreadCount = await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead);

        return View(unreadCount);
    }
}