using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Products
{
    public class UpdateProductDto
    {
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string? Name { get; set; } // Opcional en actualización

        public string? Description { get; set; }
        
        // Precios
        public decimal? BasePrice { get; set; }
        public decimal? ComparePrice { get; set; }
        public decimal? CostPerItem { get; set; }
        
        // Inventario
        public int? Stock { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public bool? TrackQuantity { get; set; }
        public bool? ContinueSellingWhenOutOfStock { get; set; }
        
        // Organización
        public string? ProductType { get; set; }
        public string? Vendor { get; set; }
        public List<string>? Tags { get; set; }
        
        // Multimedia
        public List<string>? Images { get; set; }
        
        // Envío
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public bool? RequiresShipping { get; set; }
        
        // Control
        public bool? IsActive { get; set; }
    }
}