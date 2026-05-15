using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;
namespace ProjectManagement.Models.Entities
{
    public class TeamInvitation
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        public Team Team { get; set; }

        [Required]
        public string InvitedUserId { get; set; }

        public ApplicationUser InvitedUser { get; set; }

        [Required]
        public string InvitedByUserId { get; set; }

        public ApplicationUser InvitedByUser { get; set; }

        [Required]
        public InvitationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}