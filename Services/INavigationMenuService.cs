using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.NavigationMenu;

namespace WebsiteBuilderAPI.Services
{
    public interface INavigationMenuService
    {
        Task<PagedResult<NavigationMenuResponseDto>> GetAllAsync(int companyId, int page = 1, int pageSize = 20, string? search = null);
        Task<NavigationMenuResponseDto?> GetByIdAsync(int companyId, int id);
        Task<NavigationMenuResponseDto?> GetByIdentifierAsync(int companyId, string identifier);
        Task<NavigationMenuResponseDto> CreateAsync(int companyId, CreateNavigationMenuDto dto);
        Task<NavigationMenuResponseDto?> UpdateAsync(int companyId, int id, UpdateNavigationMenuDto dto);
        Task<bool> DeleteAsync(int companyId, int id);
        Task<bool> ToggleActiveAsync(int companyId, int id);
        Task<List<NavigationMenuResponseDto>> GetActiveMenusAsync(int companyId);
        Task<bool> DuplicateAsync(int companyId, int id);
    }
}