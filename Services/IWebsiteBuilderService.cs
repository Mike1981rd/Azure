using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service interface for website builder operations
    /// </summary>
    public interface IWebsiteBuilderService
    {
        // Page operations
        Task<List<WebsitePageDto>> GetPagesByCompanyIdAsync(int companyId);
        Task<WebsitePageDto?> GetPageByIdAsync(int pageId);
        Task<WebsitePageDto?> GetPageBySlugAsync(int companyId, string slug);
        Task<WebsitePageDto> CreatePageAsync(int companyId, CreateWebsitePageDto dto);
        Task<WebsitePageDto?> UpdatePageAsync(int pageId, UpdateWebsitePageDto dto);
        Task<WebsitePageDto?> ReplacePageSectionsAsync(int pageId, UpdatePageSectionsDto dto);
        Task<bool> DeletePageAsync(int pageId);
        Task<WebsitePageDto?> DuplicatePageAsync(int pageId, string? newName = null);
        Task<WebsitePageDto?> PublishPageAsync(int pageId, PublishWebsitePageDto dto);
        Task<bool> ValidatePageAsync(int pageId);

        // Section operations
        Task<List<PageSectionDto>> GetSectionsByPageIdAsync(int pageId);
        Task<PageSectionDto?> GetSectionByIdAsync(int sectionId);
        Task<PageSectionDto> CreateSectionAsync(int pageId, CreatePageSectionDto dto);
        Task<PageSectionDto?> UpdateSectionAsync(int sectionId, UpdatePageSectionDto dto);
        Task<bool> DeleteSectionAsync(int sectionId);
        Task<PageSectionDto?> DuplicateSectionAsync(int sectionId, DuplicateSectionDto dto);
        Task<bool> ReorderSectionsAsync(int pageId, ReorderSectionsDto dto);

        // Template operations
        Task<List<WebsitePageDto>> GetPageTemplatesAsync(string? pageType = null);
        Task<WebsitePageDto> CreatePageFromTemplateAsync(int companyId, int templateId, string name);

        // Validation
        Task<bool> ValidatePageTypeAsync(string pageType);
        Task<bool> ValidateSectionTypeAsync(string sectionType);
        Task<bool> ValidateSectionConfigAsync(string sectionType, string config);
        Task<bool> CanCreatePageTypeAsync(int companyId, string pageType);
        
        // Initialization
        Task InitializeDefaultPagesAsync(int companyId);
    }
}