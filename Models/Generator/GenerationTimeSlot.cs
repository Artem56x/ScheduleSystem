using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models.Generator;

public class GenerationTimeSlot : IValidatableObject
{
    [Required]
    [Display(Name = "Начало")]
    public TimeSpan StartTime { get; set; }


    [Required]
    [Display(Name = "Конец")]
    public TimeSpan EndTime { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartTime < TimeSpan.Zero ||
            StartTime >= TimeSpan.FromDays(1))
        {
            yield return new ValidationResult(
                "Время начала должно быть в пределах суток.",
                new[] { nameof(StartTime) });
        }

        if (EndTime <= TimeSpan.Zero ||
            EndTime > TimeSpan.FromDays(1))
        {
            yield return new ValidationResult(
                "Время окончания должно быть позднее полуночи и не превышать 24:00.",
                new[] { nameof(EndTime) });
        }

        if (StartTime >= EndTime)
        {
            yield return new ValidationResult(
                "Время начала должно быть раньше времени окончания.",
                new[] { nameof(StartTime), nameof(EndTime) });
        }
    }


}
