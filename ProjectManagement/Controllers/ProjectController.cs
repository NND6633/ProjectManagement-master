using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Project;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto request)
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

            var team = await _context.Teams.FindAsync(request.TeamId);

            if (team == null)
            {
                return NotFound(new
                {
                    code = "TEAM_NOT_FOUND",
                    message = "Team not found"
                });
            }

            var hasPermission = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == request.TeamId &&
                x.UserId == currentUser.Id &&
                (x.Role == RoleInTeam.Leader || x.Role == RoleInTeam.PM));

            if (!hasPermission)
            {
                return Forbid();
            }

            var existedProject = await _context.Projects.AnyAsync(x =>
                x.TeamId == request.TeamId &&
                x.Name == request.Name);

            if (existedProject)
            {
                return BadRequest(new
                {
                    code = "PROJECT_ALREADY_EXISTS",
                    message = "Project name already exists in this team"
                });
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                Name = request.Name,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Project created successfully",
                data = new
                {
                    project.Id,
                    project.TeamId,
                    project.Name,
                    project.Status,
                    project.CreatedAt
                }
            });
        }
        [HttpGet("{id:guid}/stats")]
        public async Task<IActionResult> GetProjectStats(Guid id)
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (project == null)
            {
                return NotFound(new
                {
                    code = "PROJECT_NOT_FOUND",
                    message = "Project not found"
                });
            }

            var isTeamMember = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == project.TeamId &&
                x.UserId == currentUser.Id);

            if (!isTeamMember)
            {
                return Forbid();
            }

            var mainTaskIds = await _context.MainTasks
                .Where(x => x.ProjectId == id)
                .Select(x => x.Id)
                .ToListAsync();

            var totalMainTasks = mainTaskIds.Count;

            var totalSubtasks = await _context.Subtasks
                .CountAsync(x => mainTaskIds.Contains(x.MainTaskId));

            var completedTasks = await _context.Subtasks
                .CountAsync(x =>
                    mainTaskIds.Contains(x.MainTaskId) &&
                    x.Status == TaskItemStatus.Completed);

            var inProgressTasks = await _context.Subtasks
                .CountAsync(x =>
                    mainTaskIds.Contains(x.MainTaskId) &&
                    x.Status == TaskItemStatus.InProgress);

            var overdueTasks = await _context.Subtasks
                .CountAsync(x =>
                    mainTaskIds.Contains(x.MainTaskId) &&
                    x.Status != TaskItemStatus.Completed &&
                    x.Deadline < DateTime.UtcNow);

            var progress = totalSubtasks == 0
                ? 0
                : (float)completedTasks / totalSubtasks * 100;

            return Ok(new
            {
                message = "Project stats retrieved successfully",
                data = new
                {
                    projectId = project.Id,
                    projectName = project.Name,
                    totalMainTasks,
                    totalSubtasks,
                    completedTasks,
                    inProgressTasks,
                    overdueTasks,
                    progress
                }
            });
        }
    }
}