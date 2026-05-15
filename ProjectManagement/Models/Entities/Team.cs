using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Models.Entities
{
    public class Team
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public string LeaderId { get; set; }

        public ApplicationUser Leader { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<TeamMember> Members { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}
