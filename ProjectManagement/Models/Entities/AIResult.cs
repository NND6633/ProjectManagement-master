using ProjectManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using ProjectManagement.Models.Enums;
namespace ProjectManagement.Models.Entities
{
    public class AIResult
    {
        public Guid Id { get; set; }

        [Required]
        public AIType Type { get; set; }

        [Required]
        public string InputJson { get; set; }

        [Required]
        public string OutputJson { get; set; }

        [Range(0, 1)]
        public float ConfidenceScore { get; set; }

        public Guid? RelatedTaskId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
