using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Collections;

namespace WebsiteBuilderAPI.Services
{
    public interface ICollectionService
    {
        Task<PagedResult<CollectionListDto>> GetPagedAsync(int companyId, int page, int pageSize, string? search = null, bool? isActive = null);
        Task<CollectionDto?> GetByIdAsync(int companyId, int collectionId);
        Task<CollectionDto?> GetByHandleAsync(int companyId, string handle);
        Task<CollectionDto> CreateAsync(int companyId, CreateCollectionDto dto);
        Task<CollectionDto?> UpdateAsync(int companyId, int collectionId, UpdateCollectionDto dto);
        Task<bool> DeleteAsync(int companyId, int collectionId);
        Task<bool> BulkDeleteAsync(int companyId, List<int> collectionIds);
        
        // Product management
        Task<bool> AddProductsAsync(int companyId, int collectionId, List<int> productIds);
        Task<bool> RemoveProductsAsync(int companyId, int collectionId, List<int> productIds);
        Task<bool> UpdateProductPositionsAsync(int companyId, int collectionId, BulkUpdateProductPositionsDto dto);
        Task<List<CollectionProductDto>> GetCollectionProductsAsync(int companyId, int collectionId);
        
        // Utility methods
        Task<bool> HandleExistsAsync(int companyId, string handle, int? excludeId = null);
        Task<bool> UpdateProductCountsAsync(int companyId);
    }
}