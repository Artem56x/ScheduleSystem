using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class ScheduleGenerationSettings
{
    public int Id { get; set; }

    [Range(1, 20)]
    [Display(Name = "Количество занятий по умолчанию")]
    public int DefaultWeeklyLessons { get; set; } = 3;

    [Range(1, 10)]
    [Display(Name = "Максимум занятий в день")]
    public int MaxLessonsPerDay { get; set; } = 4;

    [Range(1, 7)]
    [Display(Name = "Количество учебных дней")]
    public int TeachingDaysPerWeek { get; set; } = 5;

    [Range(0, 10)]
    [Display(Name = "Максимум окон в день")]
    public int MaxGapsPerDay { get; set; } = 2;

    [Display(Name = "Разрешать окна")]
    public bool AllowGaps { get; set; } = true;
}