using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
    }
}