using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models.Generator;

public class GenerationTimeSlot
{
    [Required]
    [Display(Name = "Начало")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Display(Name = "Конец")]
    public TimeSpan EndTime { get; set; }
}