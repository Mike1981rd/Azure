using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.NewsletterSubscribers;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface INewsletterSubscriberService
    {
        // CRUD Operations
        Task<PagedResult<NewsletterSubscriberDto>> GetPagedAsync(int companyId, int page = 1, int pageSize = 20, string status = "", string search = "");
        Task<NewsletterSubscriberDto> GetByIdAsync(int companyId, int id);
        Task<NewsletterSubscriberDto> CreateAsync(int companyId, CreateNewsletterSubscriberDto dto);
        Task<NewsletterSubscriberDto> UpdateAsync(int companyId, int id, UpdateNewsletterSubscriberDto dto);
        Task DeleteAsync(int companyId, int id);
        
        // Public API for website forms
        Task<NewsletterSubscriberDto> PublicSubscribeAsync(PublicSubscribeDto dto);
        Task<bool> PublicUnsubscribeAsync(string email, int companyId);
        
        // Business Operations
        Task<NewsletterSubscriberDto> ConvertToCustomerAsync(int companyId, int subscriberId, ConvertToCustomerDto dto);
        Task<bool> ToggleActiveStatusAsync(int companyId, int id, bool isActive);
        Task<List<NewsletterSubscriberDto>> GetRecentSubscribersAsync(int companyId, int days = 30);
        
        // Statistics
        Task<Dictionary<string, object>> GetStatisticsAsync(int companyId);
        Task<List<NewsletterSubscriberDto>> GetByEmailAsync(int companyId, string email);
        
        // Bulk Operations
        Task<List<NewsletterSubscriberDto>> BulkUpdateStatusAsync(int companyId, List<int> subscriberIds, bool isActive);
        Task<int> BulkDeleteAsync(int companyId, List<int> subscriberIds);
    }
}