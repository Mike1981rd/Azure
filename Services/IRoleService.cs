using WebsiteBuilderAPI.DTOs.Roles;

namespace WebsiteBuilderAPI.Services
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(int id);
        Task<RoleDto> CreateAsync(CreateRoleDto dto);
        Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto);
        Task DeleteAsync(int id);
        Task AssignPermissionsAsync(int roleId, List<int> permissionIds);
        Task<bool> RoleExistsAsync(string name, int? excludeId = null);
    }
}