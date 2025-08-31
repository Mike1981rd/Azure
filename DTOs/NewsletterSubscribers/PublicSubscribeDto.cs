using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.NewsletterSubscribers
{
    public class PublicSubscribeDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        
        // Tracking parameters (optional, filled by JavaScript)
        public string? SourcePage { get; set; }
        public string? SourceCampaign { get; set; }
        public string Language { get; set; } = "es";
        
        // GDPR Consent (required for public subscription)
        [Required(ErrorMessage = "You must accept marketing communications")]
        public bool AcceptedMarketing { get; set; }
        
        [Required(ErrorMessage = "You must accept terms and conditions")]
        public bool AcceptedTerms { get; set; }
        
        // Optional: Company domain for multi-tenant (if needed later)
        public string? CompanyDomain { get; set; }
    }
}