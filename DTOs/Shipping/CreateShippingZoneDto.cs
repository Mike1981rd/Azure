using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Shipping
{
    public class CreateShippingZoneDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(domestic|international|custom)$", ErrorMessage = "ZoneType must be 'domestic', 'international', or 'custom'")]
        public string ZoneType { get; set; } = "domestic";

        [Required]
        public List<string> Countries { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;
    }

    public class CreateShippingRateDto
    {
        [Required]
        [RegularExpression("^(weight|vat|duty)$", ErrorMessage = "RateType must be 'weight', 'vat', or 'duty'")]
        public string RateType { get; set; } = "weight";

        [MaxLength(100)]
        public string? Condition { get; set; }

        [Range(0, 999999.99)]
        public decimal Price { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;
    }
}