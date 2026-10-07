namespace ScheduleSystem.ViewModels;

public class SubjectSelectionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Course { get; set; }

    public bool IsSelected { get; set; }
}