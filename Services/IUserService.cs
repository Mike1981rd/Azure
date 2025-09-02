using WebsiteBuilderAPI.DTOs.Users;

namespace WebsiteBuilderAPI.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync(int? hotelId = null);
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto dto, int? hotelId = null);
        Task<UserDto> UpdateAsync(int id, UpdateUserDto dto);
        Task DeleteAsync(int id);
        Task<List<string>> GetEffectivePermissionsAsync(int userId);
        Task<bool> UserExistsAsync(string email, int? excludeId = null);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task AssignRolesToUserAsync(int userId, List<int> roleIds);
    }
}