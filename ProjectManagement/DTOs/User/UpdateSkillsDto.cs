using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.User
{
    public class SkillDto
    {
        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; } = null!;

        [Range(1, 10)]
        public int ExperienceLevel { get; set; }
    }

    public class UpdateSkillsDto
    {
        [Required]
        public List<SkillDto> Skills { get; set; } = new();
    }
}