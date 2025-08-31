using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Contact
{
    public class ContactMessageDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsNotificationSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class CreateContactMessageDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }

    public class ContactMessageFilterDto
    {
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UpdateContactMessageStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // read, archived, unread
    }

    public class ContactNotificationSettingsDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool ToastNotificationsEnabled { get; set; } = true;
        public bool DashboardNotificationsEnabled { get; set; } = true;
        public bool PlaySoundOnNewMessage { get; set; } = false;
        public string NotificationEmailAddress { get; set; } = string.Empty;
        public string EmailSubjectTemplate { get; set; } = "New Contact Message from {name}";
        public string ToastSuccessMessage { get; set; } = "Message sent successfully!";
        public string ToastErrorMessage { get; set; } = "Error sending message. Please try again.";
    }

    public class UpdateContactNotificationSettingsDto
    {
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool ToastNotificationsEnabled { get; set; } = true;
        public bool DashboardNotificationsEnabled { get; set; } = true;
        public bool PlaySoundOnNewMessage { get; set; } = false;
        
        [EmailAddress]
        public string? NotificationEmailAddress { get; set; }
        
        [MaxLength(200)]
        public string? EmailSubjectTemplate { get; set; }
        
        [MaxLength(100)]
        public string? ToastSuccessMessage { get; set; }
        
        [MaxLength(100)]
        public string? ToastErrorMessage { get; set; }
    }
}