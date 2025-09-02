using System;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerNotificationPreferenceDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        
        // Email Notifications
        public bool EmailOrderUpdates { get; set; }
        public bool EmailPromotions { get; set; }
        public bool EmailNewsletter { get; set; }
        public bool EmailProductReviews { get; set; }
        public bool EmailPriceAlerts { get; set; }
        
        // SMS Notifications
        public bool SmsOrderUpdates { get; set; }
        public bool SmsDeliveryAlerts { get; set; }
        public bool SmsPromotions { get; set; }
        
        // Push Notifications
        public bool PushEnabled { get; set; }
        public bool PushSound { get; set; }
        public bool PushVibration { get; set; }
        
        // Notification Schedule
        public string DoNotDisturbStart { get; set; }
        public string DoNotDisturbEnd { get; set; }
        public string Timezone { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}