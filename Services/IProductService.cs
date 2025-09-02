using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Products;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponseDto>> GetProductsAsync(int companyId, int page, int pageSize, string? search, int? collectionId, bool? isActive);
        Task<ProductResponseDto?> GetProductByIdAsync(int companyId, int productId);
        Task<ProductResponseDto> CreateProductAsync(int companyId, CreateProductDto dto);
        Task<ProductResponseDto?> UpdateProductAsync(int companyId, int productId, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int companyId, int productId);
        Task<bool> BulkDeleteProductsAsync(int companyId, List<int> productIds);
        Task<bool> ToggleActiveStatusAsync(int companyId, int productId);
        Task<bool> UpdateProductImagesAsync(int companyId, int productId, List<string> imageUrls);
        Task<bool> AssignToCollectionsAsync(int companyId, int productId, List<int> collectionIds);
    }
}