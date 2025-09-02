namespace WebsiteBuilderAPI.DTOs.Notifications
{
    public class UpdateNotificationSettingDto
    {
        public bool? EmailEnabled { get; set; }
        public bool? AppEnabled { get; set; }
    }
}