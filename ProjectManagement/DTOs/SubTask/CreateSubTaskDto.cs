using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Subtask
{
    public class CreateSubTaskDto
    {
        [Required]
        public Guid MainTaskId { get; set; }

        public string? AssigneeId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}