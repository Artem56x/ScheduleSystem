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

    [Display(Name = "Курс")]
    [Range(1, 5, ErrorMessage = "Курс должен быть от 1 до 5")]
    public int Course { get; set; } = 1;

    [Display(Name = "Тип предмета")]
    public SubjectType Type { get; set; } = SubjectType.General;

    public ICollection<SubjectClassroomCategory> ClassroomCategoryRequirements { get; set; }
    = new List<SubjectClassroomCategory>();


}

