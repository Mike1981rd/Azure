using System;

namespace WebsiteBuilderAPI.DTOs.Notifications
{
    public class NotificationSettingResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool AppEnabled { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}