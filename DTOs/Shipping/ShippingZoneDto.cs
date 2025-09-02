using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Shipping
{
    public class ShippingZoneDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ZoneType { get; set; } = "domestic";
        public List<string> Countries { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public List<ShippingRateDto> Rates { get; set; } = new List<ShippingRateDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ShippingRateDto
    {
        public int Id { get; set; }
        public int ShippingZoneId { get; set; }
        public string RateType { get; set; } = "weight";
        public string? Condition { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}