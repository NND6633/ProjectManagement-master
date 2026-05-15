using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.PersonalTask
{
    public class CreatePersonalTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}