namespace ScheduleSystem.Models;

public class SubjectClassroomCategory
{
    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    public int ClassroomCategoryId { get; set; }

    public ClassroomCategory ClassroomCategory { get; set; } = null!;
}