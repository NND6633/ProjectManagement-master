using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Url]
        public string? AvatarUrl { get; set; }
    }
}