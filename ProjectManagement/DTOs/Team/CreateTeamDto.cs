using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Team
{
    public class CreateTeamDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;
    }
}
