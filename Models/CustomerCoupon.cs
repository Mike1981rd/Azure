using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerCoupon
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; }
        
        [Required]
        public decimal DiscountAmount { get; set; }
        
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } // Percentage/Fixed
        
        // Minimum purchase amount required
        public decimal? MinimumPurchase { get; set; }
        
        // Usage limits
        public int? MaxUsageCount { get; set; }
        public int UsageCount { get; set; }
        
        [Required]
        public DateTime ValidFrom { get; set; }
        
        [Required]
        public DateTime ValidUntil { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime? UsedAt { get; set; }
        
        // Applicable categories or products (JSON)
        public string? ApplicableProducts { get; set; }
        public string? ApplicableCategories { get; set; }
        
        // Navigation property
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Customer Customer { get; set; }
        
        public CustomerCoupon()
        {
            IsActive = true;
            UsageCount = 0;
            ValidFrom = DateTime.UtcNow;
            ValidUntil = DateTime.UtcNow.AddMonths(1);
        }
        
        // Helper properties
        public bool IsValid => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidUntil;
        public bool IsExpired => DateTime.UtcNow > ValidUntil;
        public bool CanBeUsed => IsValid && (MaxUsageCount == null || UsageCount < MaxUsageCount);
        
        public string GetDiscountDisplay()
        {
            if (DiscountType == "Percentage")
                return $"{DiscountAmount}% OFF";
            else
                return $"${DiscountAmount} OFF";
        }
    }
}