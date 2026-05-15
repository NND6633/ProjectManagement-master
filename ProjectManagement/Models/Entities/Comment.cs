using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Models.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        [Required]
        public Guid SubtaskId { get; set; }

        public Subtask Subtask { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
