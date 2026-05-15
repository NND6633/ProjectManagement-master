using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Team
{
    public class AssignPmDto
    {
        [Required]
        public string UserId { get; set; } = null!;
    }
}
