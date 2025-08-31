using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service interface for managing structural components (Header, Footer, Announcement Bar, Cart Drawer)
    /// </summary>
    public interface IStructuralComponentsService
    {
        // Get operations
        Task<StructuralComponentsDto?> GetByCompanyIdAsync(int companyId);
        Task<StructuralComponentsDto?> GetPublishedByCompanyIdAsync(int companyId);
        Task<string?> GetComponentConfigAsync(int companyId, string componentType);
        Task<string?> GetPublishedComponentConfigAsync(int companyId, string componentType);
        
        // Create/Update operations
        Task<StructuralComponentsDto> CreateOrUpdateAsync(int companyId, CreateStructuralComponentsDto dto);
        Task<StructuralComponentsDto?> UpdateComponentAsync(int companyId, UpdateComponentDto dto);
        Task<StructuralComponentsDto?> UpdateAllComponentsAsync(int companyId, StructuralComponentsDto dto);
        
        // Publishing operations
        Task<StructuralComponentsDto?> PublishAsync(int companyId, PublishComponentsDto dto);
        Task<StructuralComponentsDto?> UnpublishAsync(int companyId);
        Task<StructuralComponentsDto?> CreateDraftFromPublishedAsync(int companyId);
        
        // Reset operations
        Task<StructuralComponentsDto?> ResetComponentToDefaultAsync(int companyId, string componentType);
        Task<StructuralComponentsDto?> ResetAllToDefaultAsync(int companyId);
        
        // Preview operations
        Task<string> GeneratePreviewHtmlAsync(int companyId, ComponentPreviewDto dto);
        Task<string> GenerateComponentCssAsync(int companyId, string componentType);
        
        // Validation
        Task<bool> ValidateComponentConfigAsync(string componentType, string config);
        Task<ValidationResult> ValidateAllComponentsAsync(int companyId);
        
        // Version management
        Task<int> GetCurrentVersionAsync(int companyId);
        Task<StructuralComponentsDto?> GetVersionAsync(int companyId, int version);
        Task<StructuralComponentsDto[]> GetVersionHistoryAsync(int companyId, int limit = 10);
    }

    /// <summary>
    /// Validation result for components
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string[] Errors { get; set; } = System.Array.Empty<string>();
        public string[] Warnings { get; set; } = System.Array.Empty<string>();
    }
}