using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class ScheduleTimeSlot
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите время начала.")]
    [Display(Name = "Время начала")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Укажите время окончания.")]
    [Display(Name = "Время окончания")]
    public TimeSpan EndTime { get; set; }

    [Range(1, 50, ErrorMessage = "Порядок должен быть от 1 до 50.")]
    [Display(Name = "Порядок")]
    public int SortOrder { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}