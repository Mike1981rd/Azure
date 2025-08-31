using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerNotificationPreference
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        // Email Notifications
        public bool EmailOrderUpdates { get; set; } = true;
        public bool EmailPromotions { get; set; } = false;
        public bool EmailNewsletter { get; set; } = false;
        public bool EmailProductReviews { get; set; } = true;
        public bool EmailPriceAlerts { get; set; } = false;
        
        // SMS Notifications
        public bool SmsOrderUpdates { get; set; } = false;
        public bool SmsDeliveryAlerts { get; set; } = false;
        public bool SmsPromotions { get; set; } = false;
        
        // Push Notifications
        public bool PushEnabled { get; set; } = false;
        public bool PushSound { get; set; } = true;
        public bool PushVibration { get; set; } = true;
        
        // Notification Schedule
        [StringLength(10)]
        public string DoNotDisturbStart { get; set; } = "22:00";
        
        [StringLength(10)]
        public string DoNotDisturbEnd { get; set; } = "08:00";
        
        [StringLength(100)]
        public string Timezone { get; set; } = "America/Santo_Domingo";
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        
        public CustomerNotificationPreference()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}