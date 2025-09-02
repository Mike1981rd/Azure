using WebsiteBuilderAPI.DTOs.Permissions;

namespace WebsiteBuilderAPI.Services
{
    public interface IPermissionService
    {
        Task<List<PermissionDto>> GetAllAsync();
        Task<List<GroupedPermissionsDto>> GetGroupedAsync();
        Task<List<PermissionDto>> GetByResourceAsync(string resource);
        Task SeedPermissionsAsync();
    }
}