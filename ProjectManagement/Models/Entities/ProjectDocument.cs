using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models.Entities
{
    public class ProjectDocument
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileUrl { get; set; }

        [Required]
        public string UploadedBy { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
