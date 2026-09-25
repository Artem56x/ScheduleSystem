namespace ScheduleSystem.Models.Generator;

public class GenerationGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public int Course { get; set; }

    public string Specialty { get; set; } = string.Empty;

    public List<GenerationSubject> Subjects { get; set; } = new();
}