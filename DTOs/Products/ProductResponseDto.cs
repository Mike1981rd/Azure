using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Precios
        public decimal BasePrice { get; set; }
        public decimal? ComparePrice { get; set; }
        public decimal? CostPerItem { get; set; }
        
        // Inventario
        public int Stock { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public bool TrackQuantity { get; set; }
        public bool ContinueSellingWhenOutOfStock { get; set; }
        
        // Organización
        public string? ProductType { get; set; }
        public string? Vendor { get; set; }
        public List<string>? Tags { get; set; }
        
        // Multimedia
        public List<string>? Images { get; set; }
        
        // Envío
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public bool RequiresShipping { get; set; }
        
        // Control
        public bool HasVariants { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Relaciones
        public List<ProductVariantDto>? Variants { get; set; }
        public List<string>? Collections { get; set; } // Nombres de las colecciones
    }
    
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public int Stock { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public bool IsActive { get; set; }
    }
}