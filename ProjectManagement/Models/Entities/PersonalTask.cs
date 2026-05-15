using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums; 
namespace ProjectManagement.Models.Entities
{
    public class PersonalTask
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

        [Required]
        public DateTime Deadline { get; set; }
    }
}
