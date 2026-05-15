using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.MainTask;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/main-tasks")]
    [Authorize]
    public class MainTaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MainTaskController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // CREATE MAIN TASK
        // =========================
        [HttpPost]
        public async Task<IActionResult> CreateMainTask(CreateMainTaskDto request)
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

            var project = await _context.Projects
                .FirstOrDefaultAsync(x => x.Id == request.ProjectId);

            if (project == null)
            {
                return NotFound(new
                {
                    code = "PROJECT_NOT_FOUND",
                    message = "Project not found"
                });
            }

            var hasPermission = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == project.TeamId &&
                x.UserId == currentUser.Id &&
                (x.Role == RoleInTeam.Leader || x.Role == RoleInTeam.PM));

            if (!hasPermission)
            {
                return Forbid();
            }

            var mainTask = new MainTask
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                Status = TaskItemStatus.Pending,
                Progress = 0,
                Deadline = request.Deadline,
                CreatedBy = currentUser.Id
            };

            _context.MainTasks.Add(mainTask);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Main task created successfully",
                data = mainTask
            });
        }

        // =========================
        // GET MAIN TASK BY ID
        // =========================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMainTask(Guid id)
        {
            var task = await _context.MainTasks
                .Include(x => x.Subtasks)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
            {
                return NotFound(new
                {
                    code = "MAIN_TASK_NOT_FOUND",
                    message = "Main task not found"
                });
            }

            return Ok(task);
        }

        // =========================
        // UPDATE MAIN TASK STATUS
        // =========================
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, TaskItemStatus status)
        {
            var task = await _context.MainTasks.FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
            {
                return NotFound(new
                {
                    code = "MAIN_TASK_NOT_FOUND",
                    message = "Main task not found"
                });
            }

            task.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Status updated",
                data = new { task.Id, task.Status }
            });
        }

        // =========================
        // INTERNAL PROGRESS CALCULATION (NO API)
        // =========================
        private async Task RecalculateMainTaskProgress(Guid mainTaskId)
        {
            var totalSubtasks = await _context.Subtasks
                .CountAsync(x => x.MainTaskId == mainTaskId);

            var completedSubtasks = await _context.Subtasks
                .CountAsync(x =>
                    x.MainTaskId == mainTaskId &&
                    x.Status == TaskItemStatus.Completed);

            var mainTask = await _context.MainTasks
                .FirstOrDefaultAsync(x => x.Id == mainTaskId);

            if (mainTask == null) return;

            mainTask.Progress = totalSubtasks == 0
                ? 0
                : (float)completedSubtasks / totalSubtasks * 100;

            if (totalSubtasks > 0 && completedSubtasks == totalSubtasks)
            {
                mainTask.Status = TaskItemStatus.Completed;
            }
            else if (completedSubtasks > 0)
            {
                mainTask.Status = TaskItemStatus.InProgress;
            }
            else
            {
                mainTask.Status = TaskItemStatus.Pending;
            }

            await _context.SaveChangesAsync();
        }
    }
}