using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class GroupSubject
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Группа")]
    public int GroupId { get; set; }

    public Group? Group { get; set; }

    [Required]
    [Display(Name = "Предмет")]
    public int SubjectId { get; set; }

    public Subject? Subject { get; set; }

    [Range(
        1,
        20,
        ErrorMessage = "Количество пар в неделю должно быть от 1 до 20"
    )]
    [Display(Name = "Пар в неделю")]
    public int WeeklyLessons { get; set; }
}

