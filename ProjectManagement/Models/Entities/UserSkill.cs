using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Models.Entities
{
    public class UserSkill
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; }

        [Range(1, 10)]
        public int ExperienceLevel { get; set; }
    }
}
