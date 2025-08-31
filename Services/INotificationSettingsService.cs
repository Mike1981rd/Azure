using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs.Notifications;

namespace WebsiteBuilderAPI.Services
{
    public interface INotificationSettingsService
    {
        Task<List<NotificationSettingResponseDto>> GetAllByCompanyAsync(int companyId);
        Task<NotificationSettingResponseDto?> GetByIdAsync(int companyId, int id);
        Task<NotificationSettingResponseDto> CreateAsync(int companyId, CreateNotificationSettingDto dto);
        Task<NotificationSettingResponseDto?> UpdateAsync(int companyId, int id, UpdateNotificationSettingDto dto);
        Task<bool> DeleteAsync(int companyId, int id);
        Task<List<NotificationSettingResponseDto>> BulkUpdateAsync(int companyId, BulkUpdateNotificationSettingsDto dto);
        Task InitializeDefaultSettingsAsync(int companyId);
    }
}