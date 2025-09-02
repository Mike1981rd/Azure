using System;

namespace WebsiteBuilderAPI.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime GrantedAt { get; set; }

        // Navegación
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}