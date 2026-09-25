namespace ScheduleSystem.Models.Generator;

public class GenerationSubject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public SubjectType Type { get; set; }

    public int WeeklyLessons { get; set; }

    // ============================================================
    // ТРЕБОВАНИЯ К АУДИТОРИИ
    // ============================================================

    /// <summary>
    /// ID категорий аудиторий, подходящих для предмета.
    /// Если список пустой — предмет не имеет специальных
    /// требований к категории аудитории.
    /// </summary>
    public List<int> ClassroomCategoryIds { get; set; } = new();

    /// <summary>
    /// Названия категорий аудиторий.
    /// Используется для Preview и отображения пользователю.
    /// </summary>
    public List<string> ClassroomCategoryNames { get; set; } = new();
}