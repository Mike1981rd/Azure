using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Notifications
{
    public class CreateNotificationSettingDto
    {
        [Required]
        [MaxLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        public bool EmailEnabled { get; set; } = false;

        public bool AppEnabled { get; set; } = false;

        public int SortOrder { get; set; } = 0;
    }
}