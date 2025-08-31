using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class ConfigOption
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // amenity, room_type, view_type, policy_type
        
        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty; // El valor único (ej: "wifi", "pool")
        
        [Required]
        [StringLength(200)]
        public string LabelEs { get; set; } = string.Empty; // Etiqueta en español
        
        [Required]
        [StringLength(200)]
        public string LabelEn { get; set; } = string.Empty; // Etiqueta en inglés
        
        [StringLength(100)]
        public string? Icon { get; set; } // Icono (ej: "wifi", "pool", "car", etc.)
        
        [StringLength(50)]
        public string? IconType { get; set; } // Tipo de icono: "heroicon", "emoji", "custom"
        
        [StringLength(100)]
        public string? Category { get; set; } // Categoría (ej: "básicas", "premium", "exterior")
        
        public int SortOrder { get; set; } = 0; // Orden de aparición
        
        public int UsageCount { get; set; } = 0; // Contador de uso para popularidad
        
        public bool IsActive { get; set; } = true; // Si está activo
        
        public bool IsCustom { get; set; } = false; // Si fue creado por el usuario
        
        public bool IsDefault { get; set; } = false; // Si es parte del conjunto por defecto
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navegación
        public virtual Company Company { get; set; } = null!;
    }
}