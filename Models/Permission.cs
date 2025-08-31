using System.Collections.Generic;

namespace WebsiteBuilderAPI.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Resource { get; set; } = string.Empty; // dashboard, products, etc.
        public string Action { get; set; } = string.Empty; // view, read, create, update, delete
        public string Description { get; set; } = string.Empty;

        // Navegación
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}