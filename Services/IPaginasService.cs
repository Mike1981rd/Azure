using WebsiteBuilderAPI.DTOs.Paginas;
using WebsiteBuilderAPI.DTOs;

namespace WebsiteBuilderAPI.Services
{
    public interface IPaginasService
    {
        Task<DTOs.Common.ApiResponse<PagedResult<PaginaResponseDto>>> GetPaginasAsync(
            int companyId, 
            int page = 1, 
            int pageSize = 10,
            string? search = null,
            string? publishStatus = null,
            bool? isVisible = null);
            
        Task<DTOs.Common.ApiResponse<PaginaResponseDto>> GetPaginaByIdAsync(int companyId, int id);
        Task<DTOs.Common.ApiResponse<PaginaResponseDto>> GetPaginaBySlugAsync(int companyId, string slug);
        Task<DTOs.Common.ApiResponse<PaginaResponseDto>> CreatePaginaAsync(int companyId, int userId, CreatePaginaDto dto);
        Task<DTOs.Common.ApiResponse<PaginaResponseDto>> UpdatePaginaAsync(int companyId, int userId, int id, UpdatePaginaDto dto);
        Task<DTOs.Common.ApiResponse> DeletePaginaAsync(int companyId, int id);
        Task<DTOs.Common.ApiResponse> PublishPaginaAsync(int companyId, int userId, int id);
        Task<DTOs.Common.ApiResponse> UnpublishPaginaAsync(int companyId, int userId, int id);
        Task<DTOs.Common.ApiResponse<bool>> CheckSlugExistsAsync(int companyId, string slug, int? excludeId = null);
        Task<DTOs.Common.ApiResponse<string>> GenerateSlugAsync(string title);
    }
}