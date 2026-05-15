using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models.Enums;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services.Background
{
    public class DeadlineReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeadlineReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var now = DateTime.UtcNow;

                var subtasks = await context.Subtasks
                    .Where(x => x.Status != TaskItemStatus.Completed)
                    .ToListAsync(stoppingToken);

                foreach (var task in subtasks)
                {
                    var remaining = task.Deadline - now;

                    if (remaining.TotalHours <= 24 && remaining.TotalHours > 23)
                    {
                        if (!string.IsNullOrWhiteSpace(task.AssigneeId))
                        {
                            await notificationService.NotifyDeadlineAsync(task.AssigneeId, task.Title);
                        }
                    }

                    if (remaining.TotalHours <= 2 && remaining.TotalHours > 1)
                    {
                        if (!string.IsNullOrWhiteSpace(task.AssigneeId))
                        {
                            await notificationService.NotifyDeadlineAsync(task.AssigneeId, task.Title);
                        }
                    }

                    if (remaining.TotalHours < 0)
                    {
                        if (!string.IsNullOrWhiteSpace(task.AssigneeId))
                        {
                            await notificationService.NotifyOverdueAsync(task.AssigneeId, task.Title);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}