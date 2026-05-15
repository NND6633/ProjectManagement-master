using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models.Entities
{
    public class TeamMember
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        public Team Team { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        public RoleInTeam Role { get; set; }
    }
}
