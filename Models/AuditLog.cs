using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = "";

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}