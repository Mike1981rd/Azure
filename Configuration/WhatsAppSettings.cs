using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Configuration
{
    /// <summary>
    /// Configuration settings for WhatsApp integration
    /// Supports multiple providers: Twilio and Green API
    /// </summary>
    public class WhatsAppSettings
    {
        public const string SectionName = "WhatsApp";

        /// <summary>
        /// Active WhatsApp provider (Twilio or GreenAPI)
        /// </summary>
        [Required]
        public string Provider { get; set; } = WhatsAppProvider.Twilio.ToString();

        /// <summary>
        /// Twilio-specific configuration
        /// </summary>
        public TwilioSettings? Twilio { get; set; }

        /// <summary>
        /// Green API-specific configuration
        /// </summary>
        public GreenApiSettings? GreenAPI { get; set; }

        /// <summary>
        /// Common webhook settings
        /// </summary>
        public WebhookSettings Webhook { get; set; } = new();

        /// <summary>
        /// Rate limiting settings
        /// </summary>
        public RateLimitSettings RateLimit { get; set; } = new();

        /// <summary>
        /// Timeout settings for HTTP requests
        /// </summary>
        public TimeoutSettings Timeout { get; set; } = new();

        /// <summary>
        /// Validation settings
        /// </summary>
        public ValidationSettings Validation { get; set; } = new();
    }

    /// <summary>
    /// WhatsApp provider enumeration
    /// </summary>
    public enum WhatsAppProvider
    {
        /// <summary>
        /// Twilio WhatsApp Business API
        /// </summary>
        Twilio = 1,

        /// <summary>
        /// Green API WhatsApp service
        /// </summary>
        GreenAPI = 2
    }

    /// <summary>
    /// Twilio-specific configuration
    /// </summary>
    public class TwilioSettings
    {
        /// <summary>
        /// Default Account SID (can be overridden per company)
        /// </summary>
        public string? DefaultAccountSid { get; set; }

        /// <summary>
        /// Default Auth Token (can be overridden per company)
        /// </summary>
        public string? DefaultAuthToken { get; set; }

        /// <summary>
        /// Default WhatsApp phone number
        /// </summary>
        public string? DefaultPhoneNumber { get; set; }

        /// <summary>
        /// Whether to use sandbox by default
        /// </summary>
        public bool UseSandbox { get; set; } = true;

        /// <summary>
        /// Twilio API base URL
        /// </summary>
        public string ApiBaseUrl { get; set; } = "https://api.twilio.com";

        /// <summary>
        /// Twilio API version
        /// </summary>
        public string ApiVersion { get; set; } = "2010-04-01";
    }

    /// <summary>
    /// Green API-specific configuration
    /// </summary>
    public class GreenApiSettings
    {
        /// <summary>
        /// Green API base URL
        /// </summary>
        public string ApiBaseUrl { get; set; } = "https://api.green-api.com";

        /// <summary>
        /// Default instance ID (can be overridden per company)
        /// </summary>
        public string? DefaultInstanceId { get; set; }

        /// <summary>
        /// Default API token (can be overridden per company)
        /// </summary>
        public string? DefaultApiToken { get; set; }

        /// <summary>
        /// Default phone number ID
        /// </summary>
        public string? DefaultPhoneNumberId { get; set; }

        /// <summary>
        /// Webhook notification URL for receiving messages
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Green API version
        /// </summary>
        public string ApiVersion { get; set; } = "v3";

        /// <summary>
        /// Polling interval for receiving messages (in seconds)
        /// </summary>
        public int PollingIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Whether to enable webhook notifications
        /// </summary>
        public bool EnableWebhook { get; set; } = true;

        /// <summary>
        /// Whether to acknowledge received messages
        /// </summary>
        public bool AutoAcknowledgeMessages { get; set; } = true;
    }

    /// <summary>
    /// Webhook configuration
    /// </summary>
    public class WebhookSettings
    {
        /// <summary>
        /// Base URL for webhook endpoints
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Webhook verification token
        /// </summary>
        public string? VerificationToken { get; set; }

        /// <summary>
        /// Whether to validate webhook signatures
        /// </summary>
        public bool ValidateSignature { get; set; } = true;

        /// <summary>
        /// Maximum webhook payload size in bytes
        /// </summary>
        public int MaxPayloadSizeBytes { get; set; } = 1048576; // 1MB

        /// <summary>
        /// Webhook timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;
    }

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public class RateLimitSettings
    {
        /// <summary>
        /// Maximum messages per minute per company
        /// </summary>
        public int MessagesPerMinute { get; set; } = 60;

        /// <summary>
        /// Maximum messages per hour per company
        /// </summary>
        public int MessagesPerHour { get; set; } = 1000;

        /// <summary>
        /// Maximum messages per day per company
        /// </summary>
        public int MessagesPerDay { get; set; } = 10000;

        /// <summary>
        /// Whether rate limiting is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Rate limit window in minutes
        /// </summary>
        public int WindowSizeMinutes { get; set; } = 1;
    }

    /// <summary>
    /// Timeout configuration
    /// </summary>
    public class TimeoutSettings
    {
        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        public int HttpRequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Message send timeout in seconds
        /// </summary>
        public int MessageSendTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Retry delay in seconds
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Maximum retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
    }

    /// <summary>
    /// Validation settings
    /// </summary>
    public class ValidationSettings
    {
        /// <summary>
        /// Whether to validate phone numbers
        /// </summary>
        public bool ValidatePhoneNumbers { get; set; } = true;

        /// <summary>
        /// Whether to validate message content
        /// </summary>
        public bool ValidateMessageContent { get; set; } = true;

        /// <summary>
        /// Maximum message length
        /// </summary>
        public int MaxMessageLength { get; set; } = 4096;

        /// <summary>
        /// Maximum media file size in bytes
        /// </summary>
        public int MaxMediaFileSizeBytes { get; set; } = 16777216; // 16MB

        /// <summary>
        /// Allowed media file types
        /// </summary>
        public List<string> AllowedMediaTypes { get; set; } = new()
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "video/mp4",
            "audio/mpeg",
            "audio/ogg",
            "application/pdf",
            "text/plain"
        };

        /// <summary>
        /// Phone number validation regex
        /// </summary>
        public string PhoneNumberRegex { get; set; } = @"^\+[1-9]\d{10,14}$";
    }

    /// <summary>
    /// Extension methods for WhatsApp settings
    /// </summary>
    public static class WhatsAppSettingsExtensions
    {
        /// <summary>
        /// Gets the active provider as enum
        /// </summary>
        public static WhatsAppProvider GetProviderEnum(this WhatsAppSettings settings)
        {
            return Enum.TryParse<WhatsAppProvider>(settings.Provider, true, out var provider) 
                ? provider 
                : WhatsAppProvider.Twilio;
        }

        /// <summary>
        /// Checks if Twilio is the active provider
        /// </summary>
        public static bool IsTwilioActive(this WhatsAppSettings settings)
        {
            return settings.GetProviderEnum() == WhatsAppProvider.Twilio;
        }

        /// <summary>
        /// Checks if Green API is the active provider
        /// </summary>
        public static bool IsGreenApiActive(this WhatsAppSettings settings)
        {
            return settings.GetProviderEnum() == WhatsAppProvider.GreenAPI;
        }

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public static ValidationResult ValidateConfiguration(this WhatsAppSettings settings)
        {
            var errors = new List<string>();

            // Validate provider
            if (!Enum.TryParse<WhatsAppProvider>(settings.Provider, true, out _))
            {
                errors.Add($"Invalid provider: {settings.Provider}. Must be 'Twilio' or 'GreenAPI'");
            }

            // Validate provider-specific settings
            switch (settings.GetProviderEnum())
            {
                case WhatsAppProvider.Twilio:
                    if (settings.Twilio == null)
                    {
                        errors.Add("Twilio settings are required when Twilio is the active provider");
                    }
                    break;

                case WhatsAppProvider.GreenAPI:
                    if (settings.GreenAPI == null)
                    {
                        errors.Add("Green API settings are required when GreenAPI is the active provider");
                    }
                    break;
            }

            // Validate webhook settings
            if (string.IsNullOrEmpty(settings.Webhook.BaseUrl))
            {
                errors.Add("Webhook BaseUrl is required");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Configuration validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}