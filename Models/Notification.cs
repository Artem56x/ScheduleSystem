using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = "";

    [MaxLength(50)]
    public string Type { get; set; } = "info";

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = "";

    public ApplicationUser? User { get; set; }
}