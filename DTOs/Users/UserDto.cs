using WebsiteBuilderAPI.DTOs.Roles;

namespace WebsiteBuilderAPI.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public List<string> EffectivePermissions { get; set; } = new List<string>();
    }
}