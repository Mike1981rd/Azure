using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class NotificationSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // customer, orders, shipping

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        public bool EmailEnabled { get; set; } = false;

        public bool AppEnabled { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Company? Company { get; set; }
    }
}