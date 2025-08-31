using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Notifications;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationSettingsService> _logger;

        public NotificationSettingsService(ApplicationDbContext context, ILogger<NotificationSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NotificationSettingResponseDto>> GetAllByCompanyAsync(int companyId)
        {
            // Check if company has any notification settings
            var hasSettings = await _context.NotificationSettings
                .AnyAsync(n => n.CompanyId == companyId);

            // If no settings exist, initialize defaults
            if (!hasSettings)
            {
                await InitializeDefaultSettingsAsync(companyId);
            }

            var settings = await _context.NotificationSettings
                .Where(n => n.CompanyId == companyId)
                .OrderBy(n => n.Category)
                .ThenBy(n => n.SortOrder)
                .Select(n => new NotificationSettingResponseDto
                {
                    Id = n.Id,
                    CompanyId = n.CompanyId,
                    NotificationType = n.NotificationType,
                    Category = n.Category,
                    DisplayName = n.DisplayName,
                    EmailEnabled = n.EmailEnabled,
                    AppEnabled = n.AppEnabled,
                    SortOrder = n.SortOrder,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();

            return settings;
        }

        public async Task<NotificationSettingResponseDto?> GetByIdAsync(int companyId, int id)
        {
            var setting = await _context.NotificationSettings
                .Where(n => n.CompanyId == companyId && n.Id == id)
                .Select(n => new NotificationSettingResponseDto
                {
                    Id = n.Id,
                    CompanyId = n.CompanyId,
                    NotificationType = n.NotificationType,
                    Category = n.Category,
                    DisplayName = n.DisplayName,
                    EmailEnabled = n.EmailEnabled,
                    AppEnabled = n.AppEnabled,
                    SortOrder = n.SortOrder,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return setting;
        }

        public async Task<NotificationSettingResponseDto> CreateAsync(int companyId, CreateNotificationSettingDto dto)
        {
            var setting = new NotificationSettings
            {
                CompanyId = companyId,
                NotificationType = dto.NotificationType,
                Category = dto.Category,
                DisplayName = dto.DisplayName,
                EmailEnabled = dto.EmailEnabled,
                AppEnabled = dto.AppEnabled,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NotificationSettings.Add(setting);
            await _context.SaveChangesAsync();

            return new NotificationSettingResponseDto
            {
                Id = setting.Id,
                CompanyId = setting.CompanyId,
                NotificationType = setting.NotificationType,
                Category = setting.Category,
                DisplayName = setting.DisplayName,
                EmailEnabled = setting.EmailEnabled,
                AppEnabled = setting.AppEnabled,
                SortOrder = setting.SortOrder,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
        }

        public async Task<NotificationSettingResponseDto?> UpdateAsync(int companyId, int id, UpdateNotificationSettingDto dto)
        {
            var setting = await _context.NotificationSettings
                .FirstOrDefaultAsync(n => n.CompanyId == companyId && n.Id == id);

            if (setting == null)
                return null;

            if (dto.EmailEnabled.HasValue)
                setting.EmailEnabled = dto.EmailEnabled.Value;

            if (dto.AppEnabled.HasValue)
                setting.AppEnabled = dto.AppEnabled.Value;

            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new NotificationSettingResponseDto
            {
                Id = setting.Id,
                CompanyId = setting.CompanyId,
                NotificationType = setting.NotificationType,
                Category = setting.Category,
                DisplayName = setting.DisplayName,
                EmailEnabled = setting.EmailEnabled,
                AppEnabled = setting.AppEnabled,
                SortOrder = setting.SortOrder,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            var setting = await _context.NotificationSettings
                .FirstOrDefaultAsync(n => n.CompanyId == companyId && n.Id == id);

            if (setting == null)
                return false;

            _context.NotificationSettings.Remove(setting);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<NotificationSettingResponseDto>> BulkUpdateAsync(int companyId, BulkUpdateNotificationSettingsDto dto)
        {
            var settings = await _context.NotificationSettings
                .Where(n => n.CompanyId == companyId)
                .ToListAsync();

            foreach (var updateItem in dto.Settings)
            {
                var setting = settings.FirstOrDefault(s => s.NotificationType == updateItem.NotificationType);
                if (setting != null)
                {
                    setting.EmailEnabled = updateItem.EmailEnabled;
                    setting.AppEnabled = updateItem.AppEnabled;
                    setting.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return await GetAllByCompanyAsync(companyId);
        }

        public async Task InitializeDefaultSettingsAsync(int companyId)
        {
            _logger.LogInformation($"Initializing default notification settings for company {companyId}");

            var defaultSettings = new List<NotificationSettings>
            {
                // Customer notifications
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "customer_signup",
                    Category = "customer",
                    DisplayName = "New customer sign up",
                    EmailEnabled = true,
                    AppEnabled = true,
                    SortOrder = 1
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "customer_password_reset",
                    Category = "customer",
                    DisplayName = "Customer account password reset",
                    EmailEnabled = true,
                    AppEnabled = true,
                    SortOrder = 2
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "customer_account_invite",
                    Category = "customer",
                    DisplayName = "Customer account invite",
                    EmailEnabled = false,
                    AppEnabled = false,
                    SortOrder = 3
                },

                // Order notifications
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "order_purchase",
                    Category = "orders",
                    DisplayName = "Order purchase",
                    EmailEnabled = true,
                    AppEnabled = true,
                    SortOrder = 1
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "order_cancelled",
                    Category = "orders",
                    DisplayName = "Order cancelled",
                    EmailEnabled = true,
                    AppEnabled = false,
                    SortOrder = 2
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "order_refund_request",
                    Category = "orders",
                    DisplayName = "Order refund request",
                    EmailEnabled = false,
                    AppEnabled = true,
                    SortOrder = 3
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "order_confirmation",
                    Category = "orders",
                    DisplayName = "Order confirmation",
                    EmailEnabled = true,
                    AppEnabled = false,
                    SortOrder = 4
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "payment_error",
                    Category = "orders",
                    DisplayName = "Payment error",
                    EmailEnabled = true,
                    AppEnabled = false,
                    SortOrder = 5
                },

                // Shipping notifications
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "picked_up",
                    Category = "shipping",
                    DisplayName = "Picked up",
                    EmailEnabled = true,
                    AppEnabled = true,
                    SortOrder = 1
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "shipping_update",
                    Category = "shipping",
                    DisplayName = "Shipping update",
                    EmailEnabled = true,
                    AppEnabled = false,
                    SortOrder = 2
                },
                new NotificationSettings
                {
                    CompanyId = companyId,
                    NotificationType = "delivered",
                    Category = "shipping",
                    DisplayName = "Delivered",
                    EmailEnabled = false,
                    AppEnabled = true,
                    SortOrder = 3
                }
            };

            foreach (var setting in defaultSettings)
            {
                setting.CreatedAt = DateTime.UtcNow;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            _context.NotificationSettings.AddRange(defaultSettings);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Initialized {defaultSettings.Count} default notification settings for company {companyId}");
        }
    }
}