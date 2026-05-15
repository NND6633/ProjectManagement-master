namespace ProjectManagement.DTOs.User
{
    public class UserProfileResponseDto
    {
        public string Id { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public List<SkillDto> Skills { get; set; } = new();
    }
}