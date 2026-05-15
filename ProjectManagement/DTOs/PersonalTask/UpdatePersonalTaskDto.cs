using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.DTOs.PersonalTask
{
    public class UpdatePersonalTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}