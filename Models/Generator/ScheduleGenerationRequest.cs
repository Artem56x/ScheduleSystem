using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models.Generator;

public class ScheduleGenerationRequest
{
    public List<int> SelectedGroupIds { get; set; } = new();

    public List<DayOfWeek> Days { get; set; } = new();

    public List<GenerationLoadItem> Loads { get; set; } = new();

    public List<GenerationTimeSlot> TimeSlots { get; set; } = new();

    // Ниже оставь существующие настройки генерации:
    // MaxLessonsPerDay, MaxConsecutiveLessons,
    // DistributeLessons и UseClassroomRecommendations.

    [Range(
        1,
        10,
        ErrorMessage = "Максимальное количество пар в день должно быть от 1 до 10."
    )]
    [Display(Name = "Максимум пар в день")]
    public int MaxLessonsPerDay { get; set; } = 4;

    [Range(
        1,
        10,
        ErrorMessage = "Максимальное количество пар подряд должно быть от 1 до 10."
    )]
    [Display(Name = "Максимум пар подряд")]
    public int MaxConsecutiveLessons { get; set; } = 2;

    [Display(Name = "Равномерно распределять занятия")]
    public bool DistributeLessons { get; set; } = true;

    [Display(Name = "Учитывать рекомендации аудиторий")]
    public bool UseClassroomRecommendations { get; set; } = true;


}
