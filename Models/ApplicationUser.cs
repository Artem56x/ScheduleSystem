using Microsoft.AspNetCore.Identity;

namespace ScheduleSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public DateTime? LastLoginAt { get; set; }

    public string? LastLoginDevice { get; set; }

    public bool ScheduleNotificationsEnabled { get; set; } = true;

    public bool SystemNotificationsEnabled { get; set; } = true;
}