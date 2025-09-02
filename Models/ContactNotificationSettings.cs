using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Settings for contact form notifications
    /// </summary>
    public class ContactNotificationSettings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Company that owns these settings
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Enable email notifications to admin
        /// </summary>
        public bool EmailNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Enable toast notifications in UI
        /// </summary>
        public bool ToastNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Enable dashboard badge notifications
        /// </summary>
        public bool DashboardNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Play sound when new message arrives
        /// </summary>
        public bool PlaySoundOnNewMessage { get; set; } = false;

        /// <summary>
        /// Email address to send notifications to
        /// </summary>
        [StringLength(100)]
        public string? NotificationEmailAddress { get; set; }

        /// <summary>
        /// Email subject template
        /// </summary>
        [StringLength(200)]
        public string EmailSubjectTemplate { get; set; } = "New Contact Message from {name}";

        /// <summary>
        /// Toast success message
        /// </summary>
        [StringLength(200)]
        public string ToastSuccessMessage { get; set; } = "Message sent successfully!";

        /// <summary>
        /// Toast error message
        /// </summary>
        [StringLength(200)]
        public string ToastErrorMessage { get; set; } = "Error sending message. Please try again.";

        /// <summary>
        /// When settings were created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When settings were last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
    }
}