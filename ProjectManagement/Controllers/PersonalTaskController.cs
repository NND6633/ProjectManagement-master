using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.PersonalTask;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/personal-tasks")]
    [Authorize]
    public class PersonalTaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PersonalTaskController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePersonalTask(CreatePersonalTaskDto request)
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

            var task = new PersonalTask
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id,
                Title = request.Title,
                Description = request.Description,
                Deadline = request.Deadline
            };

            _context.PersonalTasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Personal task created successfully",
                data = task
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePersonalTask(Guid id, UpdatePersonalTaskDto request)
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

            var task = await _context.PersonalTasks
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.Id);

            if (task == null)
            {
                return NotFound(new
                {
                    code = "PERSONAL_TASK_NOT_FOUND",
                    message = "Personal task not found"
                });
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Deadline = request.Deadline;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Personal task updated successfully",
                data = task
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePersonalTask(Guid id)
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

            var task = await _context.PersonalTasks
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.Id);

            if (task == null)
            {
                return NotFound(new
                {
                    code = "PERSONAL_TASK_NOT_FOUND",
                    message = "Personal task not found"
                });
            }

            _context.PersonalTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Personal task deleted successfully"
            });
        }
    }
}