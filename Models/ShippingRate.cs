using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class ShippingRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShippingZoneId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RateType { get; set; } = "weight"; // "weight", "vat", "duty"

        [MaxLength(100)]
        public string? Condition { get; set; } // "5kg-10kg", "12%", "Japan", etc.

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ShippingZone ShippingZone { get; set; } = null!;
    }
}