using Microsoft.AspNetCore.Identity;
using ProjectManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Url]
        public string? AvatarUrl { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserSkill> Skills { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
