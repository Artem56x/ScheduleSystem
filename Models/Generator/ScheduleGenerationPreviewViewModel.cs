namespace ScheduleSystem.Models.Generator;

public class ScheduleGenerationPreviewViewModel
{
    // ============================================================
    // GENERATED LESSONS
    // ============================================================

    public List<GeneratedScheduleItem> Items { get; set; } = new();

    // ============================================================
    // REPLACEMENT SCOPE
    // ============================================================

    public List<int> SelectedGroupIds { get; set; } = new();

    public List<DayOfWeek> SelectedDays { get; set; } = new();

    public List<GenerationTimeSlot> TimeSlots { get; set; } = new();

    // ============================================================
    // GENERATION SETTINGS
    // ============================================================

    public int MaxLessonsPerDay { get; set; }

    public int MaxConsecutiveLessons { get; set; }

    public bool DistributeLessons { get; set; }

    public bool UseClassroomRecommendations { get; set; }

    // ============================================================
    // EXISTING DATA
    // ============================================================

    public int ExistingSchedulesToReplace { get; set; }

    // ============================================================
    // STATISTICS
    // ============================================================

    public int TotalLessons =>
        Items.Count;

    public int TotalGroups =>
        Items
            .Select(x => x.GroupId)
            .Distinct()
            .Count();

    public int TotalTeachers =>
        Items
            .Select(x => x.TeacherId)
            .Distinct()
            .Count();

    public int TotalClassrooms =>
        Items
            .Select(x => x.ClassroomId)
            .Distinct()
            .Count();

    public int TotalDays =>
        Items
            .Select(x => x.DayOfWeek)
            .Distinct()
            .Count();
}