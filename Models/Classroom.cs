using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class Classroom
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите номер аудитории")]
    [StringLength(
        50,
        ErrorMessage = "Номер аудитории не должен превышать 50 символов"
    )]
    [Display(Name = "Номер аудитории")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию аудитории")]
    [Display(Name = "Категория")]
    public int ClassroomCategoryId { get; set; }

    [Display(Name = "Категория")]
    public ClassroomCategory? ClassroomCategory { get; set; }

    [Range(
        1,
        500,
        ErrorMessage = "Количество мест должно быть от 1 до 500"
    )]
    [Display(Name = "Количество мест")]
    public int Capacity { get; set; }
}