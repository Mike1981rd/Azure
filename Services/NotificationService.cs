using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notification> CreateAsync(int companyId, string type, string title, string? message = null, object? data = null, string? relatedType = null, string? relatedId = null)
        {
            var notif = new Notification
            {
                CompanyId = companyId,
                Type = type,
                Title = title,
                Message = message,
                Data = data != null ? JsonConvert.SerializeObject(data) : null,
                RelatedEntityType = relatedType,
                RelatedEntityId = relatedId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Set<Notification>().Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<int> GetUnreadCountAsync(int companyId)
        {
            return await _context.Set<Notification>()
                .Where(n => n.CompanyId == companyId && !n.IsRead)
                .CountAsync();
        }

        public async Task<List<Notification>> GetRecentAsync(int companyId, int limit = 20)
        {
            limit = Math.Clamp(limit, 1, 100);
            return await _context.Set<Notification>()
                .Where(n => n.CompanyId == companyId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int companyId, int notificationId)
        {
            var n = await _context.Set<Notification>().FirstOrDefaultAsync(x => x.Id == notificationId && x.CompanyId == companyId);
            if (n == null) return false;
            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int companyId)
        {
            var query = _context.Set<Notification>().Where(n => n.CompanyId == companyId && !n.IsRead);
            var list = await query.ToListAsync();
            foreach (var n in list)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return list.Count;
        }
    }
}

