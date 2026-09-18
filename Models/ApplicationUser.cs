using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ScheduleSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }
    }
}