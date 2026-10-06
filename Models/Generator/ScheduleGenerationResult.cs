namespace ScheduleSystem.Models.Generator;

public class ScheduleGenerationResult
{
    public bool IsSuccess { get; set; }

    public List<GeneratedScheduleItem> GeneratedItems { get; set; } = new();

    public List<string> Errors { get; set; } = new();

    public bool HasErrors => Errors.Count > 0;

    public int GeneratedCount => GeneratedItems.Count;

    public void AddError(string message)
    {
        Errors.Add(message);
    }
}