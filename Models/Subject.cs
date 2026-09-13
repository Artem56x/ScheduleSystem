using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class Subject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название предмета")]
    [StringLength(
        100,
        ErrorMessage = "Название предмета не должно превышать 100 символов"
    )]
    [Display(Name = "Название предмета")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Требуются компьютеры")]
    public bool RequiresComputers { get; set; }
}

