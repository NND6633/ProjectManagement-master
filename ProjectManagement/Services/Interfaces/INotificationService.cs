using ProjectManagement.Models.Enums;

namespace ProjectManagement.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, NotificationType type, string content);

        Task NotifyTaskAssignedAsync(string userId, string taskTitle);

        Task NotifyCommentAsync(string userId, string taskTitle);

        Task NotifyDeadlineAsync(string userId, string taskTitle);

        Task NotifyOverdueAsync(string userId, string taskTitle);
    }
}