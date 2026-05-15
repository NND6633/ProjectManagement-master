using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Models.Entities
{
    public class Subtask
    {
        public Guid Id { get; set; }

        [Required]
        public Guid MainTaskId { get; set; }

        public MainTask MainTask { get; set; }

        public string? AssigneeId { get; set; }

        public ApplicationUser Assignee { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

        [Required]
        public DateTime Deadline { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
