using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class NewsletterSubscriber
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Tracking information
        [StringLength(255)]
        public string? SourcePage { get; set; } // URL where they subscribed
        
        [StringLength(50)]
        public string? SourceCampaign { get; set; } // Marketing campaign
        
        [StringLength(50)]
        public string Language { get; set; } = "es"; // Newsletter language preference
        
        // GDPR & Privacy
        public bool AcceptedMarketing { get; set; } = true;
        public bool AcceptedTerms { get; set; } = true;
        public DateTime? OptInDate { get; set; }
        public DateTime? OptOutDate { get; set; }
        
        [StringLength(255)]
        public string? UnsubscribeReason { get; set; }
        
        // Customer conversion tracking
        public bool ConvertedToCustomer { get; set; } = false;
        public int? CustomerId { get; set; } // Reference to Customer if converted
        public DateTime? ConvertedAt { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Soft delete
        
        // Navigation properties
        [JsonIgnore]
        public virtual Company Company { get; set; }
        
        [JsonIgnore]
        public virtual Customer Customer { get; set; } // If converted
        
        public NewsletterSubscriber()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            OptInDate = DateTime.UtcNow;
        }
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsConverted => ConvertedToCustomer && CustomerId.HasValue;
        public int DaysSinceSubscription => (DateTime.UtcNow - CreatedAt).Days;
    }
}