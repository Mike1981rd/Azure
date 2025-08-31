using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Notifications
{
    public class BulkUpdateNotificationSettingsDto
    {
        public List<NotificationUpdateItem> Settings { get; set; } = new List<NotificationUpdateItem>();
    }

    public class NotificationUpdateItem
    {
        public string NotificationType { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool AppEnabled { get; set; }
    }
}