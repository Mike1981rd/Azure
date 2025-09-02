using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// WhatsApp provider information DTO
    /// </summary>
    public class WhatsAppProviderInfoDto
    {
        /// <summary>
        /// Currently active provider name
        /// </summary>
        public string ActiveProvider { get; set; } = string.Empty;

        /// <summary>
        /// Provider service name
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// List of available providers
        /// </summary>
        public List<string> AvailableProviders { get; set; } = new();

        /// <summary>
        /// Whether the provider is configured for the company
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Company ID checked
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// When the check was performed
        /// </summary>
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// Test message request DTO
    /// </summary>
    public class WhatsAppTestMessageRequestDto
    {
        /// <summary>
        /// Phone number to send test message to
        /// </summary>
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Custom message to send (optional, will use default if not provided)
        /// </summary>
        [StringLength(1000)]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Test message result DTO
    /// </summary>
    public class WhatsAppTestMessageResultDto
    {
        /// <summary>
        /// Whether the test message was sent successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Provider used to send the message
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Message ID from the provider
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Phone number the message was sent to
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Content of the message that was sent
        /// </summary>
        public string? MessageContent { get; set; }

        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Duration in milliseconds to send the message
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Message status from provider
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Error message if sending failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Provider switch request DTO
    /// </summary>
    public class WhatsAppProviderSwitchRequestDto
    {
        /// <summary>
        /// Name of the provider to switch to
        /// </summary>
        [Required]
        public string ProviderName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provider switch result DTO
    /// </summary>
    public class WhatsAppProviderSwitchResultDto
    {
        /// <summary>
        /// Whether the switch was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Previous provider name
        /// </summary>
        public string PreviousProvider { get; set; } = string.Empty;

        /// <summary>
        /// New provider name
        /// </summary>
        public string NewProvider { get; set; } = string.Empty;

        /// <summary>
        /// Whether the new provider is configured
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Company ID for the switch
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// When the switch occurred
        /// </summary>
        public DateTime SwitchedAt { get; set; }

        /// <summary>
        /// Result message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error message if switch failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Single provider health status DTO
    /// </summary>
    public class WhatsAppProviderHealthDto
    {
        /// <summary>
        /// Provider name
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this provider is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether this provider is configured
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Health status (Healthy, Not Configured, Error)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Error message if status is Error
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When the health check was performed
        /// </summary>
        public DateTime LastChecked { get; set; }
    }

    /// <summary>
    /// All providers health status DTO
    /// </summary>
    public class WhatsAppProvidersHealthDto
    {
        /// <summary>
        /// Overall health status (Healthy, Degraded, Down)
        /// </summary>
        public string OverallStatus { get; set; } = string.Empty;

        /// <summary>
        /// Currently active provider
        /// </summary>
        public string ActiveProvider { get; set; } = string.Empty;

        /// <summary>
        /// Company ID checked
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Individual provider statuses
        /// </summary>
        public List<WhatsAppProviderHealthDto> Providers { get; set; } = new();

        /// <summary>
        /// When the health check was performed
        /// </summary>
        public DateTime CheckedAt { get; set; }
    }

}