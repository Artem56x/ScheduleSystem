using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public enum SubjectType
{
    [Display(Name = "Общеобразовательный")]
    General = 0,

    [Display(Name = "Специальный")]
    Special = 1
}

