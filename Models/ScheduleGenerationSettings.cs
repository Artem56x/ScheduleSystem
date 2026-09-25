using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class ScheduleGenerationSettings
{
    public int Id { get; set; }

    [Range(
        1,
        60,
        ErrorMessage = "Стандартная недельная нагрузка должна быть от 1 до 60 пар."
    )]
    [Display(Name = "Стандартная недельная нагрузка")]
    public int DefaultWeeklyLessons { get; set; } = 20;

    [Range(
        1,
        12,
        ErrorMessage = "Максимум пар в день должен быть от 1 до 12."
    )]
    [Display(Name = "Максимум пар в день")]
    public int MaxLessonsPerDay { get; set; } = 4;

    [Range(
        1,
        7,
        ErrorMessage = "Количество учебных дней должно быть от 1 до 7."
    )]
    [Display(Name = "Учебных дней в неделю")]
    public int TeachingDaysPerWeek { get; set; } = 5;

    [Display(Name = "Разрешать окна")]
    public bool AllowGaps { get; set; } = true;

    [Range(
        0,
        6,
        ErrorMessage = "Максимум окон в день должен быть от 0 до 6."
    )]
    [Display(Name = "Максимум окон в день")]
    public int MaxGapsPerDay { get; set; } = 1;
}