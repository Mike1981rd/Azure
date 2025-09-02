using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Rooms
{
    public class UpdateRoomDto
    {
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }

        // Precios
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal? BasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de comparación debe ser mayor o igual a 0")]
        public decimal? ComparePrice { get; set; }

        // Información específica
        [Range(1, 20, ErrorMessage = "La ocupación debe estar entre 1 y 20 personas")]
        public int? MaxOccupancy { get; set; }

        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        public string? RoomCode { get; set; }

        [StringLength(100, ErrorMessage = "El tipo no puede exceder 100 caracteres")]
        public string? RoomType { get; set; }

        [Range(0, 200, ErrorMessage = "El piso debe estar entre 0 y 200")]
        public int? FloorNumber { get; set; }

        [StringLength(100, ErrorMessage = "El tipo de vista no puede exceder 100 caracteres")]
        public string? ViewType { get; set; }

        [Range(0, 10000, ErrorMessage = "Los metros cuadrados deben estar entre 0 y 10000")]
        public decimal? SquareMeters { get; set; }

        // NUEVOS - Ubicación
        [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
        public string? StreetAddress { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "El estado no puede exceder 100 caracteres")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        public string? Country { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
        public string? PostalCode { get; set; }

        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        public decimal? Longitude { get; set; }

        [StringLength(100, ErrorMessage = "El vecindario no puede exceder 100 caracteres")]
        public string? Neighborhood { get; set; }

        // NUEVO - Host
        public int? HostId { get; set; }

        // NUEVOS - Configuraciones JSON
        public object? SleepingArrangements { get; set; }
        public object? HouseRules { get; set; }
        public object? CancellationPolicy { get; set; }
        public object? CheckInInstructions { get; set; }
        public object? SafetyFeatures { get; set; }
        public object? HighlightFeatures { get; set; }
        public object? AdditionalFees { get; set; }
        public object? SafetyAndProperty { get; set; }
        public object? GuestMaximum { get; set; }
        public object? RoomDetails { get; set; }
        public object? CommonSpaces { get; set; }
        public object? Highlights { get; set; } // Puntos destacados de la habitación

        // Órdenes de visualización para secciones de políticas
        public List<string>? HouseRulesOrder { get; set; }
        public List<string>? CancellationPolicyOrder { get; set; }
        public List<string>? SafetyAndPropertyOrder { get; set; }

        // Organización
        public List<string>? Tags { get; set; }
        public List<string>? Amenities { get; set; }

        // Multimedia
        public List<string>? Images { get; set; }

        // NUEVOS - SEO
        [StringLength(200, ErrorMessage = "El slug no puede exceder 200 caracteres")]
        public string? Slug { get; set; }

        [StringLength(200, ErrorMessage = "El título meta no puede exceder 200 caracteres")]
        public string? MetaTitle { get; set; }

        [StringLength(500, ErrorMessage = "La descripción meta no puede exceder 500 caracteres")]
        public string? MetaDescription { get; set; }

        // Control
        public bool? IsActive { get; set; }
    }
}