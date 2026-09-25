namespace ScheduleSystem.Models.Generator;

public class GeneratedScheduleItem
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public int ClassroomId { get; set; }

    public string ClassroomName { get; set; } = string.Empty;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int LessonNumber { get; set; }
}