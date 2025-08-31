using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class CheckoutSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        // Customer Contact Method
        [Required]
        [MaxLength(20)]
        public string ContactMethod { get; set; } = "email"; // "phone" or "email"

        // Full Name Configuration  
        [Required]
        [MaxLength(20)]
        public string FullNameOption { get; set; } = "firstAndLast"; // "lastOnly" or "firstAndLast"

        // Field Requirements
        [Required]
        [MaxLength(20)]
        public string CompanyNameField { get; set; } = "optional"; // "hidden", "optional", "required"

        [Required]
        [MaxLength(20)]
        public string AddressLine2Field { get; set; } = "optional"; // "hidden", "optional", "required"

        [Required]
        [MaxLength(20)]
        public string PhoneNumberField { get; set; } = "optional"; // "hidden", "optional", "required"

        // Additional Settings (for future expansion)
        public bool RequireShippingAddress { get; set; } = true;
        public bool RequireBillingAddress { get; set; } = true;
        public bool AllowGuestCheckout { get; set; } = true;
        public bool CollectMarketingConsent { get; set; } = false;
        
        // Terms and Conditions
        public bool ShowTermsAndConditions { get; set; } = true;
        public string? TermsAndConditionsUrl { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Checkout Branding (Logo for Checkout Header)
        // Optional dedicated logo for checkout. If null, use Company.Logo
        public string? CheckoutLogoUrl { get; set; }

        // Visual alignment of the checkout logo in the header: left | center | right
        public string? CheckoutLogoAlignment { get; set; } = "center";

        // Preferred width in pixels for checkout logo (UI can clamp)
        public int? CheckoutLogoWidthPx { get; set; }

        // Pay button colors
        [MaxLength(20)]
        public string? CheckoutPayButtonColor { get; set; } = "#22c55e"; // default primary

        [MaxLength(20)]
        public string? CheckoutPayButtonTextColor { get; set; } = "#ffffff";

        // Navigation property
        public virtual Company Company { get; set; } = null!;
    }
}