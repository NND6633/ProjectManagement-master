using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Models.Entities
{
    public class MainTask
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public Project Project { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

        [Range(0, 100)]
        public float Progress { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public ApplicationUser Creator { get; set; }

        public ICollection<Subtask> Subtasks { get; set; }
    }
}
