using System;
using System.Collections.Generic;
using System.Text.Json;

namespace WebsiteBuilderAPI.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Precios
        public decimal BasePrice { get; set; }
        public decimal? ComparePrice { get; set; } // Precio anterior (para tachar)
        
        // Información específica de habitación
        public int MaxOccupancy { get; set; }
        public string? RoomCode { get; set; } // Código único de habitación (ej: "SUITE-101")
        public string? RoomType { get; set; } // Tipo: Estándar, Suite, Deluxe, etc.
        public int? FloorNumber { get; set; } // Número de piso
        public string? ViewType { get; set; } // Vista: Mar, Ciudad, Jardín, etc.
        public decimal? SquareMeters { get; set; } // Tamaño en metros cuadrados
        
        // NUEVOS CAMPOS - Ubicación para mapa
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Neighborhood { get; set; }
        
        // NUEVO - Relación con Host
        public int? HostId { get; set; }
        public virtual Host? Host { get; set; }
        
        // NUEVOS CAMPOS JSONB - Configuraciones complejas
        public JsonDocument? SleepingArrangements { get; set; } // Configuración de camas y habitaciones
        public JsonDocument? HouseRules { get; set; } // Reglas de la casa
        public JsonDocument? CancellationPolicy { get; set; } // Política de cancelación
        public JsonDocument? CheckInInstructions { get; set; } // Instrucciones de check-in
        public JsonDocument? SafetyFeatures { get; set; } // Características de seguridad
        public JsonDocument? HighlightFeatures { get; set; } // Características destacadas
        public JsonDocument? AdditionalFees { get; set; } // Tarifas adicionales
        public JsonDocument? SafetyAndProperty { get; set; } // Seguridad y propiedad - Rich text
        public JsonDocument? GuestMaximum { get; set; } // Límites de huéspedes
        public JsonDocument? RoomDetails { get; set; } // Detalles de la habitación
        public JsonDocument? CommonSpaces { get; set; } // Espacios comunes
        public JsonDocument? Highlights { get; set; } // Puntos destacados de la habitación (location, check-in, wifi, parking, etc.)
        
        // Órdenes de visualización para secciones de políticas (persistentes)
        public List<string>? HouseRulesOrder { get; set; }
        public List<string>? CancellationPolicyOrder { get; set; }
        public List<string>? SafetyAndPropertyOrder { get; set; }

        // Organización y búsqueda
        public List<string>? Tags { get; set; } // JSONB para búsquedas
        public List<string>? Amenities { get; set; } // JSONB ["WiFi", "Minibar", "Jacuzzi", etc.]
        
        // Multimedia
        public List<string>? Images { get; set; } // URLs de imágenes (JSONB)
        
        // NUEVOS - SEO y Marketing
        public string? Slug { get; set; } // URL amigable
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        
        // NUEVOS - Estadísticas calculadas
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal? ResponseRate { get; set; }
        public string? ResponseTime { get; set; }
        
        // Control
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navegación
        public Company Company { get; set; } = null!;
    }
}