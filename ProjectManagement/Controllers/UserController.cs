using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Common;
using ProjectManagement.Data;
using ProjectManagement.DTOs.User;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /api/v1/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "USER_NOT_FOUND",
                        "User not found",
                        401));
            }

            var skills = await _context.UserSkills
                .Where(s => s.UserId == currentUser.Id)
                .Select(s => new SkillDto
                {
                    SkillName = s.SkillName,
                    ExperienceLevel = s.ExperienceLevel
                })
                .ToListAsync();

            var response = new UserProfileResponseDto
            {
                Id = currentUser.Id,
                Email = currentUser.Email!,
                FullName = currentUser.FullName,
                AvatarUrl = currentUser.AvatarUrl,
                Status = currentUser.Status,
                CreatedAt = currentUser.CreatedAt,
                Skills = skills
            };

            return Ok(
                ApiResponse<UserProfileResponseDto>.SuccessResponse(
                    response,
                    "Profile retrieved successfully"));
        }

        // PUT: /api/v1/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "VALIDATION_ERROR",
                        "Invalid request data"));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "USER_NOT_FOUND",
                        "User not found",
                        401));
            }

            currentUser.FullName = request.FullName.Trim();
            currentUser.AvatarUrl = request.AvatarUrl?.Trim();

            var result = await _userManager.UpdateAsync(currentUser);

            if (!result.Succeeded)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "UPDATE_PROFILE_FAILED",
                        "Failed to update profile"));
            }

            _logger.LogInformation(
                "User {UserId} updated profile",
                currentUser.Id);

            var response = new UserProfileResponseDto
            {
                Id = currentUser.Id,
                Email = currentUser.Email!,
                FullName = currentUser.FullName,
                AvatarUrl = currentUser.AvatarUrl,
                Status = currentUser.Status,
                CreatedAt = currentUser.CreatedAt
            };

            return Ok(
                ApiResponse<UserProfileResponseDto>.SuccessResponse(
                    response,
                    "Profile updated successfully"));
        }

        // PUT: /api/v1/users/skills
        [HttpPut("skills")]
        public async Task<IActionResult> UpdateSkills(
            [FromBody] UpdateSkillsDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "VALIDATION_ERROR",
                        "Invalid request data"));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "USER_NOT_FOUND",
                        "User not found",
                        401));
            }

            if (request.Skills.Count > 20)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "SKILL_LIMIT_EXCEEDED",
                        "Maximum 20 skills allowed"));
            }

            var hasDuplicateSkills = request.Skills
                .GroupBy(s => s.SkillName.Trim().ToLower())
                .Any(g => g.Count() > 1);

            if (hasDuplicateSkills)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "DUPLICATE_SKILL",
                        "Duplicate skills are not allowed"));
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var existingSkills = await _context.UserSkills
                    .Where(s => s.UserId == currentUser.Id)
                    .ToListAsync();

                _context.UserSkills.RemoveRange(existingSkills);

                var newSkills = request.Skills.Select(s => new UserSkill
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    SkillName = s.SkillName.Trim(),
                    ExperienceLevel = s.ExperienceLevel
                });

                await _context.UserSkills.AddRangeAsync(newSkills);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "User {UserId} updated skills",
                    currentUser.Id);

                var responseSkills = await _context.UserSkills
                    .Where(s => s.UserId == currentUser.Id)
                    .Select(s => new SkillDto
                    {
                        SkillName = s.SkillName,
                        ExperienceLevel = s.ExperienceLevel
                    })
                    .ToListAsync();

                return Ok(
                    ApiResponse<List<SkillDto>>.SuccessResponse(
                        responseSkills,
                        "Skills updated successfully"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Failed to update skills for user {UserId}",
                    currentUser.Id);

                return StatusCode(500,
                    ApiResponse<object>.Fail(
                        "INTERNAL_SERVER_ERROR",
                        "Something went wrong",
                        500));
            }
        }

        // GET: /api/v1/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _context.Users
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "USER_NOT_FOUND",
                        "User not found",
                        404));
            }

            var response = new UserProfileResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                Skills = user.Skills.Select(s => new SkillDto
                {
                    SkillName = s.SkillName,
                    ExperienceLevel = s.ExperienceLevel
                }).ToList()
            };

            return Ok(
                ApiResponse<UserProfileResponseDto>.SuccessResponse(
                    response,
                    "User retrieved successfully"));
        }
    }
}