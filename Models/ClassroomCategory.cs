using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class ClassroomCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название категории")]
    [StringLength(100, ErrorMessage = "Название категории не должно превышать 100 символов")]
    [Display(Name = "Название категории")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Classroom> Classrooms { get; set; }
        = new List<Classroom>();

    public ICollection<SubjectClassroomCategory> SubjectRequirements { get; set; }
= new List<SubjectClassroomCategory>();
}