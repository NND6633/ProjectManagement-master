using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Subtask;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;
using ProjectManagement.Services;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/subtasks")]
    [Authorize]
    public class SubtaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly MainTaskService _mainTaskService;
        public SubtaskController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            MainTaskService mainTaskService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _mainTaskService = mainTaskService;
        }
        public SubtaskController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubtask(CreateSubTaskDto request)
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

            var mainTask = await _context.MainTasks
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == request.MainTaskId);

            if (mainTask == null)
            {
                return NotFound(new
                {
                    code = "MAIN_TASK_NOT_FOUND",
                    message = "Main task not found"
                });
            }

            var hasPermission = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == mainTask.Project.TeamId &&
                x.UserId == currentUser.Id &&
                (x.Role == RoleInTeam.Leader || x.Role == RoleInTeam.PM));

            if (!hasPermission)
            {
                return Forbid();
            }

            if (mainTask.Status == TaskItemStatus.Completed)
            {
                return BadRequest(new
                {
                    code = "MAIN_TASK_COMPLETED",
                    message = "Cannot create subtask under completed main task"
                });
            }

            if (request.Deadline > mainTask.Deadline)
            {
                return BadRequest(new
                {
                    code = "INVALID_DEADLINE",
                    message = "Subtask deadline cannot exceed main task deadline"
                });
            }

            if (!string.IsNullOrWhiteSpace(request.AssigneeId))
            {
                var isValidAssignee = await _context.TeamMembers.AnyAsync(x =>
                    x.TeamId == mainTask.Project.TeamId &&
                    x.UserId == request.AssigneeId);

                if (!isValidAssignee)
                {
                    return BadRequest(new
                    {
                        code = "INVALID_ASSIGNEE",
                        message = "Assignee is not a member of this team"
                    });
                }
            }

            var subtask = new Subtask
            {
                Id = Guid.NewGuid(),
                MainTaskId = request.MainTaskId,
                AssigneeId = request.AssigneeId,
                Title = request.Title,
                Description = request.Description,
                Status = TaskItemStatus.Pending,
                Deadline = request.Deadline
            };

            _context.Subtasks.Add(subtask);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(subtask.AssigneeId))
            {
                await _notificationService.NotifyTaskAssignedAsync(
                    subtask.AssigneeId,
                    subtask.Title
                );
            }
            return Ok(new
            {
                message = "Subtask created successfully",
                data = new
                {
                    subtask.Id,
                    subtask.MainTaskId,
                    subtask.AssigneeId,
                    subtask.Title,
                    subtask.Description,
                    subtask.Status,
                    subtask.Deadline
                }
            });
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateSubtaskStatus(Guid id, [FromBody] TaskItemStatus status)
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subtask == null)
            {
                return NotFound(new
                {
                    code = "SUBTASK_NOT_FOUND",
                    message = "Subtask not found"
                });
            }

            var teamRole = await _context.TeamMembers
                .Where(x => x.TeamId == subtask.MainTask.Project.TeamId && x.UserId == currentUser.Id)
                .Select(x => x.Role)
                .FirstOrDefaultAsync();

            var isAssignee = subtask.AssigneeId == currentUser.Id;
            var isManager = teamRole == RoleInTeam.Leader || teamRole == RoleInTeam.PM;

            if (!isAssignee && !isManager)
            {
                return Forbid();
            }

            subtask.Status = status;
            await _context.SaveChangesAsync();

            if (subtask.AssigneeId != null && subtask.AssigneeId != currentUser.Id)
            {
                await _notificationService.NotifyCommentAsync(
                    subtask.AssigneeId,
                    subtask.Title
                );
            }

            var mainTaskController = HttpContext.RequestServices.GetRequiredService<MainTaskController>();
            await _mainTaskService.RecalculateProgress(subtask.MainTaskId);

            return Ok(new
            {
                message = "Subtask status updated successfully",
                data = new
                {
                    subtask.Id,
                    subtask.Status
                }
            });
        }
    }
}