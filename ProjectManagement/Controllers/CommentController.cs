using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/comments")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class CreateCommentDto
        {
            [Required]
            public Guid SubtaskId { get; set; }

            [Required]
            [MaxLength(1000)]
            public string Content { get; set; } = null!;
        }

        public class UpdateCommentDto
        {
            [Required]
            [MaxLength(1000)]
            public string Content { get; set; } = null!;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDto request)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var subtask = await _context.Subtasks
                .Include(x => x.MainTask)
                .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == request.SubtaskId);

            if (subtask == null)
            {
                return NotFound(new
                {
                    code = "SUBTASK_NOT_FOUND",
                    message = "Subtask not found"
                });
            }

            var isTeamMember = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == subtask.MainTask.Project.TeamId &&
                x.UserId == currentUser.Id);

            if (!isTeamMember)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                SubtaskId = request.SubtaskId,
                UserId = currentUser.Id,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment created successfully",
                data = comment
            });
        }

        [HttpGet("subtask/{subtaskId:guid}")]
        public async Task<IActionResult> GetCommentsBySubtask(Guid subtaskId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var subtask = await _context.Subtasks
                .Include(x => x.MainTask)
                .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == subtaskId);

            if (subtask == null)
            {
                return NotFound(new
                {
                    code = "SUBTASK_NOT_FOUND",
                    message = "Subtask not found"
                });
            }

            var isTeamMember = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == subtask.MainTask.Project.TeamId &&
                x.UserId == currentUser.Id);

            if (!isTeamMember)
            {
                return Forbid();
            }

            var comments = await _context.Comments
                .Where(x => x.SubtaskId == subtaskId)
                .Include(x => x.User)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.SubtaskId,
                    x.UserId,
                    userName = x.User.FullName,
                    x.Content,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Comments retrieved successfully",
                data = comments
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateComment(Guid id, UpdateCommentDto request)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null)
            {
                return NotFound(new
                {
                    code = "COMMENT_NOT_FOUND",
                    message = "Comment not found"
                });
            }

            if (comment.UserId != currentUser.Id)
            {
                return Forbid();
            }

            comment.Content = request.Content;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment updated successfully",
                data = comment
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var comment = await _context.Comments
                .Include(x => x.Subtask)
                .ThenInclude(x => x.MainTask)
                .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null)
            {
                return NotFound(new
                {
                    code = "COMMENT_NOT_FOUND",
                    message = "Comment not found"
                });
            }

            var isOwner = comment.UserId == currentUser.Id;

            var isLeader = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == comment.Subtask.MainTask.Project.TeamId &&
                x.UserId == currentUser.Id &&
                x.Role == RoleInTeam.Leader);

            if (!isOwner && !isLeader)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment deleted successfully"
            });
        }
    }
}