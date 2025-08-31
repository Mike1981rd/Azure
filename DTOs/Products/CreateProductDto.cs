using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Products
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        // Todos los demás campos son opcionales
        public string? Description { get; set; }
        
        // Precios (opcionales)
        public decimal? BasePrice { get; set; }
        public decimal? ComparePrice { get; set; }
        public decimal? CostPerItem { get; set; }
        
        // Inventario (opcionales)
        public int? Stock { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public bool? TrackQuantity { get; set; }
        public bool? ContinueSellingWhenOutOfStock { get; set; }
        
        // Organización (opcionales)
        public string? ProductType { get; set; }
        public string? Vendor { get; set; }
        public List<string>? Tags { get; set; }
        
        // Multimedia (opcionales)
        public List<string>? Images { get; set; }
        
        // Envío (opcionales)
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public bool? RequiresShipping { get; set; }
        
        // Control (opcionales)
        public bool? IsActive { get; set; }
    }
}