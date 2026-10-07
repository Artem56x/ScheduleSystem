using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class Teacher
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите ФИО преподавателя.")]
    [StringLength(150)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    public ICollection<TeacherSubject> TeacherSubjects { get; set; }
        = new List<TeacherSubject>();

    [EmailAddress(ErrorMessage = "Введите корректный Email.")]
    [StringLength(254)]
    [Display(Name = "Email")]
    public string? Email { get; set; }
}