using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Shipping
{
    public class UpdateShippingZoneDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [RegularExpression("^(domestic|international|custom)$", ErrorMessage = "ZoneType must be 'domestic', 'international', or 'custom'")]
        public string? ZoneType { get; set; }

        public List<string>? Countries { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }

    public class UpdateShippingRateDto
    {
        [RegularExpression("^(weight|vat|duty)$", ErrorMessage = "RateType must be 'weight', 'vat', or 'duty'")]
        public string? RateType { get; set; }

        [MaxLength(100)]
        public string? Condition { get; set; }

        [Range(0, 999999.99)]
        public decimal? Price { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }

    public class BulkUpdateShippingDto
    {
        public List<ShippingZoneUpdateItem> Zones { get; set; } = new List<ShippingZoneUpdateItem>();
    }

    public class ShippingZoneUpdateItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ZoneType { get; set; } = string.Empty;
        public List<string> Countries { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public List<ShippingRateUpdateItem> Rates { get; set; } = new List<ShippingRateUpdateItem>();
    }

    public class ShippingRateUpdateItem
    {
        public int Id { get; set; }
        public string RateType { get; set; } = string.Empty;
        public string? Condition { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}