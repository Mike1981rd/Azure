using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class CollectionProduct
    {
        public int Id { get; set; }
        
        [Required]
        public int CollectionId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        // Position of product within the collection (for custom sorting)
        public int Position { get; set; } = 0;
        
        // Whether this product is featured in the collection
        public bool IsFeatured { get; set; } = false;
        
        // Date when product was added to collection
        public DateTime AddedAt { get; set; }
        
        // Navigation properties
        public virtual Collection Collection { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        
        public CollectionProduct()
        {
            AddedAt = DateTime.UtcNow;
        }
    }
}