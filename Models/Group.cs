using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название группы")]
        [StringLength(50, ErrorMessage = "Название группы не должно превышать 50 символов")]
        [Display(Name = "Название группы")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите специальность")]
        [StringLength(150, ErrorMessage = "Специальность не должна превышать 150 символов")]
        [Display(Name = "Специальность")]
        public string Specialty { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Количество человек должно быть от 1 до 100")]
        [Display(Name = "Количество человек")]
        public int StudentCount { get; set; }

        public string? Description { get; set; }
    }
}

