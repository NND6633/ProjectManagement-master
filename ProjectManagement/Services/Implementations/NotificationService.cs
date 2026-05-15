using ProjectManagement.Data;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(string userId, NotificationType type, string content)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyTaskAssignedAsync(string userId, string taskTitle)
        {
            await CreateAsync(
                userId,
                NotificationType.TaskAssigned,
                $"You have been assigned to task: {taskTitle}"
            );
        }

        public async Task NotifyCommentAsync(string userId, string taskTitle)
        {
            await CreateAsync(
                userId,
                NotificationType.Comment,
                $"A new comment was added in task: {taskTitle}"
            );
        }

        public async Task NotifyDeadlineAsync(string userId, string taskTitle)
        {
            await CreateAsync(
                userId,
                NotificationType.DeadlineReminder,
                $"Task deadline is approaching: {taskTitle}"
            );
        }

        public async Task NotifyOverdueAsync(string userId, string taskTitle)
        {
            await CreateAsync(
                userId,
                NotificationType.Overdue,
                $"Task is overdue: {taskTitle}"
            );
        }
    }
}