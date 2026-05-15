using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Team
{
    public class InviteMemberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}