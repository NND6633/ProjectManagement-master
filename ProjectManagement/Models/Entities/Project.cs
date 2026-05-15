using ProjectManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models.Entities
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        public Team Team { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;

        public DateTime CreatedAt { get; set; }

        public ICollection<MainTask> MainTasks { get; set; }
        public ICollection<ProjectDocument> Documents { get; set; }
    }
}
