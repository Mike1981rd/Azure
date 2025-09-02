namespace WebsiteBuilderAPI.DTOs.CheckoutSettings
{
    public class CheckoutSettingsDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        // Customer Contact Method
        public string ContactMethod { get; set; } = "email";
        
        // Full Name Configuration
        public string FullNameOption { get; set; } = "firstAndLast";
        
        // Field Requirements
        public string CompanyNameField { get; set; } = "optional";
        public string AddressLine2Field { get; set; } = "optional";
        public string PhoneNumberField { get; set; } = "optional";
        
        // Additional Settings
        public bool RequireShippingAddress { get; set; } = true;
        public bool RequireBillingAddress { get; set; } = true;
        public bool AllowGuestCheckout { get; set; } = true;
        public bool CollectMarketingConsent { get; set; } = false;
        
        // Terms and Conditions
        public bool ShowTermsAndConditions { get; set; } = true;
        public string? TermsAndConditionsUrl { get; set; }
        
        // Branding
        public string? CheckoutLogoUrl { get; set; }
        public string? CheckoutLogoAlignment { get; set; }
        public int? CheckoutLogoWidthPx { get; set; }
        public string? CheckoutPayButtonColor { get; set; }
        public string? CheckoutPayButtonTextColor { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}