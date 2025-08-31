namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class UpdateNotificationPreferencesDto
    {
        // Email Notifications
        public EmailNotificationsDto EmailNotifications { get; set; }
        
        // SMS Notifications
        public SmsNotificationsDto SmsNotifications { get; set; }
        
        // Push Notifications
        public PushNotificationsDto PushNotifications { get; set; }
        
        // Notification Schedule
        public NotificationScheduleDto NotificationSchedule { get; set; }
        
        public UpdateNotificationPreferencesDto()
        {
            EmailNotifications = new EmailNotificationsDto();
            SmsNotifications = new SmsNotificationsDto();
            PushNotifications = new PushNotificationsDto();
            NotificationSchedule = new NotificationScheduleDto();
        }
    }
    
    public class EmailNotificationsDto
    {
        public bool OrderUpdates { get; set; } = true;
        public bool Promotions { get; set; } = false;
        public bool Newsletter { get; set; } = false;
        public bool ProductReviews { get; set; } = true;
        public bool PriceAlerts { get; set; } = false;
    }
    
    public class SmsNotificationsDto
    {
        public bool OrderUpdates { get; set; } = false;
        public bool DeliveryAlerts { get; set; } = false;
        public bool Promotions { get; set; } = false;
    }
    
    public class PushNotificationsDto
    {
        public bool Enabled { get; set; } = false;
        public bool Sound { get; set; } = true;
        public bool Vibration { get; set; } = true;
    }
    
    public class NotificationScheduleDto
    {
        public string DoNotDisturbStart { get; set; } = "22:00";
        public string DoNotDisturbEnd { get; set; } = "08:00";
        public string Timezone { get; set; } = "America/Santo_Domingo";
    }
}