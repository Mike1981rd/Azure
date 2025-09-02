using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Rooms
{
    public class RoomResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Precios
        public decimal BasePrice { get; set; }
        public decimal? ComparePrice { get; set; }

        // Información específica
        public int MaxOccupancy { get; set; }
        public string? RoomCode { get; set; }
        public string? RoomType { get; set; }
        public int? FloorNumber { get; set; }
        public string? ViewType { get; set; }
        public decimal? SquareMeters { get; set; }

        // NUEVOS - Ubicación
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Neighborhood { get; set; }

        // NUEVO - Host
        public int? HostId { get; set; }
        public object? Host { get; set; } // Incluirá info básica del host

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
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        // NUEVOS - Estadísticas
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal? ResponseRate { get; set; }
        public string? ResponseTime { get; set; }

        // Control
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}