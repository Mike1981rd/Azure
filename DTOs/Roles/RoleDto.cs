using WebsiteBuilderAPI.DTOs.Permissions;

namespace WebsiteBuilderAPI.DTOs.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public int UserCount { get; set; }
        public List<string> Avatars { get; set; } = new List<string>(); // URLs de avatares de usuarios con este rol
        public DateTime CreatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}