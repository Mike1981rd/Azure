using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// WhatsApp Twilio configuration for each company
    /// </summary>
    public class WhatsAppConfig
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Company that owns this configuration
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Provider type: Twilio or GreenAPI
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Provider { get; set; } = "Twilio";

        /// <summary>
        /// Twilio Account SID (encrypted) - Required only for Twilio
        /// </summary>
        [StringLength(500)]
        public string? TwilioAccountSid { get; set; } = string.Empty;

        /// <summary>
        /// Twilio Auth Token (encrypted) - Required only for Twilio
        /// </summary>
        [StringLength(500)]
        public string? TwilioAuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Green API Instance ID - Required only for GreenAPI
        /// </summary>
        [StringLength(100)]
        public string? GreenApiInstanceId { get; set; }

        /// <summary>
        /// Green API Token (encrypted) - Required only for GreenAPI
        /// </summary>
        [StringLength(500)]
        public string? GreenApiToken { get; set; }

        /// <summary>
        /// Green API Token Mask for display
        /// </summary>
        [StringLength(100)]
        public string? GreenApiTokenMask { get; set; }

        /// <summary>
        /// Twilio Account SID Mask for display
        /// </summary>
        [StringLength(100)]
        public string? TwilioAccountSidMask { get; set; }

        /// <summary>
        /// Twilio Auth Token Mask for display
        /// </summary>
        [StringLength(100)]
        public string? TwilioAuthTokenMask { get; set; }

        /// <summary>
        /// WhatsApp Business Phone Number (from Twilio)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string WhatsAppPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Base webhook URL for receiving messages
        /// </summary>
        [Required]
        [StringLength(500)]
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Whether WhatsApp integration is active
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Secret used to authorize incoming webhooks (encrypted at rest)
        /// </summary>
        [StringLength(500)]
        public string? WebhookSecret { get; set; }

        /// <summary>
        /// Header name used for webhook authorization validation (default: Authorization)
        /// </summary>
        [StringLength(100)]
        public string HeaderName { get; set; } = "Authorization";

        /// <summary>
        /// Header value template, supports {secret} placeholder. Default: "Bearer {secret}"
        /// </summary>
        [StringLength(200)]
        public string HeaderValueTemplate { get; set; } = "Bearer {secret}";

        /// <summary>
        /// Whether to use Twilio sandbox (for testing)
        /// </summary>
        public bool UseSandbox { get; set; } = true;

        /// <summary>
        /// Auto-reply settings (JSON)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AutoReplySettings { get; set; }

        /// <summary>
        /// Business hours configuration (JSON)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? BusinessHours { get; set; }

        /// <summary>
        /// Message templates (JSON)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? MessageTemplates { get; set; }

        /// <summary>
        /// Webhook verification token
        /// </summary>
        [StringLength(100)]
        public string? WebhookToken { get; set; }

        /// <summary>
        /// Rate limit per minute (default: 60)
        /// </summary>
        public int RateLimitPerMinute { get; set; } = 60;

        /// <summary>
        /// Rate limit per hour (default: 1000)
        /// </summary>
        public int RateLimitPerHour { get; set; } = 1000;

        /// <summary>
        /// Maximum retry attempts for failed messages
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Retry delay in minutes
        /// </summary>
        public int RetryDelayMinutes { get; set; } = 5;

        /// <summary>
        /// Additional configuration settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AdditionalSettings { get; set; }

        /// <summary>
        /// When configuration was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When configuration was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time configuration was tested successfully
        /// </summary>
        public DateTime? LastTestedAt { get; set; }

        /// <summary>
        /// Test result message
        /// </summary>
        [StringLength(500)]
        public string? LastTestResult { get; set; }

        /// <summary>
        /// Last time an inbound webhook event was received
        /// </summary>
        public DateTime? LastWebhookEventAt { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
    }

    /// <summary>
    /// Auto-reply settings structure
    /// </summary>
    public class AutoReplySettings
    {
        public bool Enabled { get; set; } = false;
        public string WelcomeMessage { get; set; } = string.Empty;
        public string OutOfHoursMessage { get; set; } = string.Empty;
        public int DelaySeconds { get; set; } = 30;
        public bool OnlyForFirstMessage { get; set; } = true;
    }

    /// <summary>
    /// Business hours configuration
    /// </summary>
    public class BusinessHours
    {
        public bool Enabled { get; set; } = false;
        public string Timezone { get; set; } = "America/Santo_Domingo";
        public List<BusinessDay> Days { get; set; } = new();
    }

    public class BusinessDay
    {
        public string Day { get; set; } = string.Empty; // Monday, Tuesday, etc.
        public bool IsOpen { get; set; } = true;
        public string OpenTime { get; set; } = "09:00";
        public string CloseTime { get; set; } = "18:00";
    }

    /// <summary>
    /// Message templates for quick replies
    /// </summary>
    public class MessageTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "general";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
