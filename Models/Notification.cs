using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // e.g., reservation_paid, contact_message, subscription

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Message { get; set; }

        [Column(TypeName = "jsonb")]
        public string? Data { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Optional linkage for quick navigation
        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        [StringLength(100)]
        public string? RelatedEntityId { get; set; }
    }
}

