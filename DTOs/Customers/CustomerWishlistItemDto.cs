using System;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerWishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public bool InStock { get; set; }
        public DateTime AddedAt { get; set; }
        public bool NotifyOnPriceChange { get; set; }
        public bool NotifyOnBackInStock { get; set; }
    }
}