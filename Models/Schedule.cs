using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models;

public class Schedule
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Пожалуйста, выберите преподавателя.")]
    [Display(Name = "Преподаватель")]
    public int TeacherId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Пожалуйста, выберите группу.")]
    [Display(Name = "Группа")]
    public int GroupId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Пожалуйста, выберите предмет.")]
    [Display(Name = "Предмет")]
    public int SubjectId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Пожалуйста, выберите аудиторию.")]
    [Display(Name = "Аудитория")]
    public int ClassroomId { get; set; }

    [Required(ErrorMessage = "Пожалуйста, выберите день недели.")]
    [Display(Name = "День недели")]
    public DayOfWeek? DayOfWeek { get; set; }

    [Required(ErrorMessage = "Пожалуйста, укажите время начала.")]
    [Display(Name = "Время начала")]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "Пожалуйста, укажите время окончания.")]
    [Display(Name = "Время окончания")]
    public TimeSpan? EndTime { get; set; }

    public Teacher? Teacher { get; set; }
    public Group? Group { get; set; }
    public Subject? Subject { get; set; }
    public Classroom? Classroom { get; set; }
}

