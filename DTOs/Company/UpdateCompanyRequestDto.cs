using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Company
{
    public class UpdateCompanyRequestDto
    {
        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Logo { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "PrimaryColor must be a valid hex color")]
        public string? PrimaryColor { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "SecondaryColor must be a valid hex color")]
        public string? SecondaryColor { get; set; }

        // Profile Section
        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? ContactEmail { get; set; }

        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? SenderEmail { get; set; }

        // Billing Information
        [StringLength(255)]
        public string? LegalBusinessName { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Apartment { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        // Time Zone & Units
        [StringLength(100)]
        public string? TimeZone { get; set; }

        [StringLength(20)]
        public string? MetricSystem { get; set; }

        [StringLength(20)]
        public string? WeightUnit { get; set; }

        // Store Currency
        [StringLength(3)]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter ISO code")]
        public string? Currency { get; set; }

        // Order ID Format
        [StringLength(10)]
        public string? OrderIdPrefix { get; set; }

        [StringLength(10)]
        public string? OrderIdSuffix { get; set; }

        // Maps & Geolocation
        [StringLength(50)]
        public string? GeolocationProvider { get; set; }

        [StringLength(512)]
        public string? GeolocationToken { get; set; }
    }
}