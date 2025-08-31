using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// DTO for WhatsApp configuration display (sensitive data hidden)
    /// </summary>
    public class WhatsAppConfigDto
    {
        public Guid Id { get; set; }
        public int CompanyId { get; set; }
        
        // Provider field
        public string Provider { get; set; } = "Twilio";
        
        // Twilio fields
        public string? TwilioAccountSidMask { get; set; } = string.Empty; // Only last 4 chars
        public string? TwilioAccountSid { get; set; } = string.Empty; // Full value for editing
        public string? TwilioAuthToken { get; set; } = string.Empty; // For editing
        
        // Green API fields
        public string? GreenApiInstanceId { get; set; } = string.Empty;
        public string? GreenApiToken { get; set; } = string.Empty;
        public string? GreenApiTokenMask { get; set; } = string.Empty; // Only last 4 chars
        
        // Common fields
        public string WhatsAppPhoneNumber { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool UseSandbox { get; set; }
        public AutoReplySettingsDto? AutoReplySettings { get; set; }
        public BusinessHoursDto? BusinessHours { get; set; }
        public List<MessageTemplateDto>? MessageTemplates { get; set; }
        public int RateLimitPerMinute { get; set; }
        public int RateLimitPerHour { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int RetryDelayMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastTestedAt { get; set; }
        public string? LastTestResult { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating WhatsApp configuration (Multi-Provider)
    /// </summary>
    public class CreateWhatsAppConfigDto
    {
        // Provider selection
        [Required]
        [StringLength(20)]
        public string Provider { get; set; } = "Twilio";
        
        // Twilio fields (required only for Twilio provider)
        [StringLength(100)]
        public string? TwilioAccountSid { get; set; }

        [StringLength(100)]
        public string? TwilioAuthToken { get; set; }

        // Green API fields (required only for Green API provider)
        [StringLength(50)]
        public string? GreenApiInstanceId { get; set; }

        [StringLength(100)]
        public string? GreenApiToken { get; set; }

        // Common fields
        [Phone]
        [StringLength(20)]
        public string? WhatsAppPhoneNumber { get; set; }

        [Url]
        [StringLength(500)]
        public string? WebhookUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = false;
        public bool UseSandbox { get; set; } = true;

        public AutoReplySettingsDto? AutoReplySettings { get; set; }
        public BusinessHoursDto? BusinessHours { get; set; }
        
        [StringLength(100)]
        public string? WebhookToken { get; set; }

        [Range(1, 1000)]
        public int RateLimitPerMinute { get; set; } = 60;

        [Range(1, 10000)]
        public int RateLimitPerHour { get; set; } = 1000;

        [Range(1, 10)]
        public int MaxRetryAttempts { get; set; } = 3;

        [Range(1, 60)]
        public int RetryDelayMinutes { get; set; } = 5;
        
        /// <summary>
        /// Validate that either Twilio OR Green API fields are provided, not both
        /// </summary>
        public bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();
            
            bool hasTwilioFields = !string.IsNullOrEmpty(TwilioAccountSid) && !string.IsNullOrEmpty(TwilioAuthToken);
            bool hasGreenApiFields = !string.IsNullOrEmpty(GreenApiInstanceId) && !string.IsNullOrEmpty(GreenApiToken);
            
            if (!hasTwilioFields && !hasGreenApiFields)
            {
                validationErrors.Add("Either Twilio credentials (Account SID + Auth Token) or Green API credentials (Instance ID + API Token) must be provided.");
            }
            
            if (hasTwilioFields && hasGreenApiFields)
            {
                validationErrors.Add("Cannot configure both Twilio and Green API at the same time. Choose one provider.");
            }
            
            if (string.IsNullOrEmpty(WhatsAppPhoneNumber))
            {
                validationErrors.Add("WhatsApp phone number is required.");
            }
            
            return validationErrors.Count == 0;
        }
    }

    /// <summary>
    /// DTO for updating WhatsApp configuration (Multi-Provider)
    /// </summary>
    public class UpdateWhatsAppConfigDto
    {
        // Provider selection
        [StringLength(20)]
        public string? Provider { get; set; }
        
        // Twilio fields
        [StringLength(100)]
        public string? TwilioAccountSid { get; set; }

        [StringLength(100)]
        public string? TwilioAuthToken { get; set; }

        // Green API fields
        [StringLength(50)]
        public string? GreenApiInstanceId { get; set; }

        [StringLength(100)]
        public string? GreenApiToken { get; set; }

        // Common fields
        [Phone]
        [StringLength(20)]
        public string? WhatsAppPhoneNumber { get; set; }

        [Url]
        [StringLength(500)]
        public string? WebhookUrl { get; set; }

        public bool? IsActive { get; set; }
        public bool? UseSandbox { get; set; }

        public AutoReplySettingsDto? AutoReplySettings { get; set; }
        public BusinessHoursDto? BusinessHours { get; set; }
        
        [StringLength(100)]
        public string? WebhookToken { get; set; }

        [Range(1, 1000)]
        public int? RateLimitPerMinute { get; set; }

        [Range(1, 10000)]
        public int? RateLimitPerHour { get; set; }

        [Range(1, 10)]
        public int? MaxRetryAttempts { get; set; }

        [Range(1, 60)]
        public int? RetryDelayMinutes { get; set; }
    }

    /// <summary>
    /// DTO for auto-reply settings
    /// </summary>
    public class AutoReplySettingsDto
    {
        public bool Enabled { get; set; } = false;

        [StringLength(1000)]
        public string WelcomeMessage { get; set; } = string.Empty;

        [StringLength(1000)]
        public string OutOfHoursMessage { get; set; } = string.Empty;

        [Range(0, 300)]
        public int DelaySeconds { get; set; } = 30;

        public bool OnlyForFirstMessage { get; set; } = true;
    }

    /// <summary>
    /// DTO for business hours configuration
    /// </summary>
    public class BusinessHoursDto
    {
        public bool Enabled { get; set; } = false;

        [StringLength(50)]
        public string Timezone { get; set; } = "America/Santo_Domingo";

        public List<BusinessDayDto> Days { get; set; } = new();
    }

    /// <summary>
    /// DTO for business day configuration
    /// </summary>
    public class BusinessDayDto
    {
        [Required]
        [StringLength(10)]
        public string Day { get; set; } = string.Empty; // Monday, Tuesday, etc.

        public bool IsOpen { get; set; } = true;

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
        public string OpenTime { get; set; } = "09:00";

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
        public string CloseTime { get; set; } = "18:00";
    }

    /// <summary>
    /// DTO for testing WhatsApp configuration
    /// </summary>
    public class TestWhatsAppConfigDto
    {
        [Required]
        [Phone]
        public string TestPhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string TestMessage { get; set; } = "Prueba de configuración de WhatsApp - Twilio Integration";
    }

    /// <summary>
    /// DTO for configuration test result
    /// </summary>
    public class WhatsAppConfigTestResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TwilioMessageSid { get; set; }
        public DateTime TestedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Details { get; set; }
    }
}