using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.NewsletterSubscribers
{
    public class UpdateNewsletterSubscriberDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }
        
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }
        
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }
        
        public bool? IsActive { get; set; }
        
        [StringLength(5, ErrorMessage = "Language must be 2-5 characters (e.g., 'es', 'en')")]
        public string? Language { get; set; }
        
        public bool? AcceptedMarketing { get; set; }
        
        [StringLength(255, ErrorMessage = "Unsubscribe reason cannot exceed 255 characters")]
        public string? UnsubscribeReason { get; set; }
    }
}