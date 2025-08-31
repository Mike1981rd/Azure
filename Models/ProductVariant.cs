using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Precios
        public decimal Price { get; set; }
        public decimal? ComparePrice { get; set; }
        public decimal? CostPerItem { get; set; }
        
        // Inventario
        public int Stock { get; set; }
        public string? SKU { get; set; } // SKU específico de la variante
        public string? Barcode { get; set; } // Barcode específico de la variante
        
        // Atributos y multimedia
        public string? Attributes { get; set; } // JSON almacenado como string
        public string? ImageUrl { get; set; } // Imagen específica de la variante
        
        // Envío
        public decimal? Weight { get; set; } // Peso específico de la variante
        
        // Control
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navegación
        [JsonIgnore]
        public Product Product { get; set; } = null!;

        // Helper para trabajar con atributos
        public Dictionary<string, string> GetAttributes()
        {
            if (string.IsNullOrEmpty(Attributes))
                return new Dictionary<string, string>();
            
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(Attributes) 
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public void SetAttributes(Dictionary<string, string> attributes)
        {
            Attributes = JsonSerializer.Serialize(attributes);
        }
    }
}