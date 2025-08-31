using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int? CompanyId { get; set; } // Para multi-tenancy
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public Company? Company { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Propiedades computadas
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}