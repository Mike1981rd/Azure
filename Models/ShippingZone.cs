using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class ShippingZone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ZoneType { get; set; } = "domestic"; // "domestic", "international", "custom"

        // Store countries as JSON array in PostgreSQL
        public List<string> Countries { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<ShippingRate> Rates { get; set; } = new List<ShippingRate>();
    }
}