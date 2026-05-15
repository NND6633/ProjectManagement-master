using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.MainTask
{
    public class CreateMainTaskDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}