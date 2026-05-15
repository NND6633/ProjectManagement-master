using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }
}