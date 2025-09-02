using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerWishlistItem
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; } // Optional variant
        
        public DateTime AddedAt { get; set; }
        
        // Notification preference for this item
        public bool NotifyOnPriceChange { get; set; }
        public bool NotifyOnBackInStock { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        [JsonIgnore]
        public virtual Product Product { get; set; }
        [JsonIgnore]
        public virtual ProductVariant? ProductVariant { get; set; }
        
        public CustomerWishlistItem()
        {
            AddedAt = DateTime.UtcNow;
            NotifyOnPriceChange = true;
            NotifyOnBackInStock = true;
        }
    }
}