using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(int companyId, string type, string title, string? message = null, object? data = null, string? relatedType = null, string? relatedId = null);
        Task<int> GetUnreadCountAsync(int companyId);
        Task<List<Notification>> GetRecentAsync(int companyId, int limit = 20);
        Task<bool> MarkAsReadAsync(int companyId, int notificationId);
        Task<int> MarkAllAsReadAsync(int companyId);
    }
}

