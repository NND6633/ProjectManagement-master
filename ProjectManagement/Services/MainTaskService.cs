using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models.Enums;

namespace ProjectManagement.Services
{
    public class MainTaskService
    {
        private readonly AppDbContext _context;

        public MainTaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateProgress(Guid mainTaskId)
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
                mainTask.Status = TaskItemStatus.Completed;
            else if (completedSubtasks > 0)
                mainTask.Status = TaskItemStatus.InProgress;
            else
                mainTask.Status = TaskItemStatus.Pending;

            await _context.SaveChangesAsync();
        }
    }
}