namespace ScheduleSystem.Models.Generator;

public class ScheduleGenerationPreviewViewModel
{
    public List<GeneratedScheduleItem> Items { get; set; } = new();

    public int TotalLessons => Items.Count;

    public int TotalGroups =>
        Items.Select(x => x.GroupId)
            .Distinct()
            .Count();

    public int TotalTeachers =>
        Items.Select(x => x.TeacherId)
            .Distinct()
            .Count();

    public int TotalClassrooms =>
        Items.Select(x => x.ClassroomId)
            .Distinct()
            .Count();

    public int TotalDays =>
        Items.Select(x => x.DayOfWeek)
            .Distinct()
            .Count();
}