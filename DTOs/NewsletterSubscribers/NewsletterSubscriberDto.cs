using System;

namespace WebsiteBuilderAPI.DTOs.NewsletterSubscribers
{
    public class NewsletterSubscriberDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        
        // Tracking
        public string SourcePage { get; set; }
        public string SourceCampaign { get; set; }
        public string Language { get; set; }
        
        // Privacy & GDPR
        public bool AcceptedMarketing { get; set; }
        public bool AcceptedTerms { get; set; }
        public DateTime? OptInDate { get; set; }
        public DateTime? OptOutDate { get; set; }
        public string UnsubscribeReason { get; set; }
        
        // Customer conversion
        public bool ConvertedToCustomer { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? ConvertedAt { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Computed
        public int DaysSinceSubscription { get; set; }
        public bool IsConverted { get; set; }
    }
}