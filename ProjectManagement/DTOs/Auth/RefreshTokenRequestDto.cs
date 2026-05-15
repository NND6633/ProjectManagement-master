using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}