using System;

namespace WebsiteBuilderAPI.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public int? AssignedByUserId { get; set; }

        // Navegación
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
        public User? AssignedByUser { get; set; }
    }
}