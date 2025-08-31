using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerAddress
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // Home/Office/Family/Other
        
        [Required]
        [StringLength(100)]
        public string Label { get; set; } // Display name like "23 Shatinon Mekalan"
        
        [Required]
        [StringLength(255)]
        public string Street { get; set; }
        
        [StringLength(100)]
        public string? Apartment { get; set; }
        
        [Required]
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(100)]
        public string? State { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Country { get; set; }
        
        [StringLength(20)]
        public string? PostalCode { get; set; }
        
        public bool IsDefault { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        
        public CustomerAddress()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}