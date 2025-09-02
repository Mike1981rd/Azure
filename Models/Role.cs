using System.Collections.Generic;

namespace WebsiteBuilderAPI.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; } = false; // Roles del sistema no se pueden eliminar
        public DateTime CreatedAt { get; set; }

        // Navegación
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Roles predefinidos del sistema
        public const string SuperAdmin = "SuperAdmin";
        public const string CompanyAdmin = "CompanyAdmin";
        public const string CompanyStaff = "CompanyStaff";
        public const string Customer = "Customer";
    }
}