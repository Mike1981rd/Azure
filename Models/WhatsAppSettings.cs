namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Configuration settings for WhatsApp providers
    /// </summary>
    public class WhatsAppSettings
    {
        /// <summary>
        /// Active provider: "Twilio" or "GreenAPI"
        /// </summary>
        public string Provider { get; set; } = "GreenAPI";

        /// <summary>
        /// Twilio configuration
        /// </summary>
        public TwilioSettings Twilio { get; set; } = new();

        /// <summary>
        /// GREEN-API configuration
        /// </summary>
        public GreenApiSettings GreenAPI { get; set; } = new();
    }

    /// <summary>
    /// Twilio specific configuration
    /// </summary>
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public bool UseProductionCredentials { get; set; } = false;
    }

    /// <summary>
    /// GREEN-API specific configuration
    /// </summary>
    public class GreenApiSettings
    {
        public string IdInstance { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.green-api.com";
        public string WebhookUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }


    /// <summary>
    /// Phone number validation result
    /// </summary>
    public class PhoneValidationResult
    {
        public bool IsValid { get; set; }
        public string? FormattedNumber { get; set; }
        public string? ErrorMessage { get; set; }
        public string Provider { get; set; } = string.Empty;
    }
}