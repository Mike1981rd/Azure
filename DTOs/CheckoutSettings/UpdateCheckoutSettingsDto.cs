using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.CheckoutSettings
{
    public class UpdateCheckoutSettingsDto
    {
        [Required]
        [RegularExpression("^(phone|email)$", ErrorMessage = "ContactMethod must be 'phone' or 'email'")]
        public string ContactMethod { get; set; } = "email";
        
        [Required]
        [RegularExpression("^(lastOnly|firstAndLast)$", ErrorMessage = "FullNameOption must be 'lastOnly' or 'firstAndLast'")]
        public string FullNameOption { get; set; } = "firstAndLast";
        
        [Required]
        [RegularExpression("^(hidden|optional|required)$", ErrorMessage = "CompanyNameField must be 'hidden', 'optional', or 'required'")]
        public string CompanyNameField { get; set; } = "optional";
        
        [Required]
        [RegularExpression("^(hidden|optional|required)$", ErrorMessage = "AddressLine2Field must be 'hidden', 'optional', or 'required'")]
        public string AddressLine2Field { get; set; } = "optional";
        
        [Required]
        [RegularExpression("^(hidden|optional|required)$", ErrorMessage = "PhoneNumberField must be 'hidden', 'optional', or 'required'")]
        public string PhoneNumberField { get; set; } = "optional";
        
        public bool RequireShippingAddress { get; set; } = true;
        public bool RequireBillingAddress { get; set; } = true;
        public bool AllowGuestCheckout { get; set; } = true;
        public bool CollectMarketingConsent { get; set; } = false;
        public bool ShowTermsAndConditions { get; set; } = true;
        
        [MaxLength(500)]
        public string? TermsAndConditionsUrl { get; set; }

        // Branding
        // Accept relative (e.g., /uploads/checkout/...) or absolute URLs
        public string? CheckoutLogoUrl { get; set; }

        [RegularExpression("^(left|center|right)$", ErrorMessage = "CheckoutLogoAlignment must be 'left', 'center' or 'right'")]
        public string? CheckoutLogoAlignment { get; set; } = "center";

        [Range(40, 400)]
        public int? CheckoutLogoWidthPx { get; set; }

        [RegularExpression("^#?[0-9A-Fa-f]{3,8}$", ErrorMessage = "Invalid hex color")]
        public string? CheckoutPayButtonColor { get; set; }

        [RegularExpression("^#?[0-9A-Fa-f]{3,8}$", ErrorMessage = "Invalid hex color")]
        public string? CheckoutPayButtonTextColor { get; set; }
    }
}