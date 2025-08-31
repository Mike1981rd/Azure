using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty; // ÚNICO CAMPO REQUERIDO
        public string? Description { get; set; }
        
        // Precios (todos opcionales, default 0)
        public decimal BasePrice { get; set; } = 0;
        public decimal? ComparePrice { get; set; } // Precio comparado (tachado)
        public decimal? CostPerItem { get; set; } // Costo por artículo
        
        // Inventario (todos opcionales)
        public int Stock { get; set; } = 0;
        public string? SKU { get; set; } // Código de inventario
        public string? Barcode { get; set; } // Código de barras
        public bool TrackQuantity { get; set; } = true; // Rastrear cantidad
        public bool ContinueSellingWhenOutOfStock { get; set; } = false;
        
        // Organización
        public string? ProductType { get; set; } // Tipo de producto
        public string? Vendor { get; set; } // Proveedor
        public List<string>? Tags { get; set; } // Etiquetas (JSONB)
        
        // Multimedia
        public List<string>? Images { get; set; } // URLs de imágenes (JSONB)
        
        // Envío
        public decimal? Weight { get; set; } // Peso del producto
        public string? WeightUnit { get; set; } = "kg"; // Unidad de peso (kg, lb, oz, g)
        public bool RequiresShipping { get; set; } = true; // Es un producto físico
        
        // Control
        public bool HasVariants { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navegación
        [JsonIgnore]
        public Company Company { get; set; } = null!;
        [JsonIgnore]
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        [JsonIgnore]
        public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
    }
}