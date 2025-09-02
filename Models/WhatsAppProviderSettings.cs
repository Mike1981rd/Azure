using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Enhanced WhatsApp configuration model that supports multiple providers
    /// Extends the existing WhatsAppConfig to maintain backward compatibility
    /// </summary>
    public class WhatsAppProviderSettings : WhatsAppConfig
    {
        /// <summary>
        /// WhatsApp provider type (Twilio, GreenAPI)
        /// </summary>
        [StringLength(20)]
        public string Provider { get; set; } = "Twilio";

        /// <summary>
        /// Green API Instance ID (encrypted)
        /// </summary>
        [StringLength(500)]
        public string? GreenApiInstanceId { get; set; }

        /// <summary>
        /// Green API Token (encrypted)
        /// </summary>
        [StringLength(500)]
        public string? GreenApiToken { get; set; }

        /// <summary>
        /// Green API Phone Number ID
        /// </summary>
        [StringLength(50)]
        public string? GreenApiPhoneNumberId { get; set; }

        /// <summary>
        /// Green API webhook URL
        /// </summary>
        [StringLength(500)]
        public string? GreenApiWebhookUrl { get; set; }

        /// <summary>
        /// Green API polling interval in seconds
        /// </summary>
        public int GreenApiPollingIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Whether to enable Green API webhook
        /// </summary>
        public bool GreenApiEnableWebhook { get; set; } = true;

        /// <summary>
        /// Whether to auto-acknowledge messages in Green API
        /// </summary>
        public bool GreenApiAutoAcknowledge { get; set; } = true;

        /// <summary>
        /// Provider-specific configuration (JSON)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? ProviderConfig { get; set; }

        /// <summary>
        /// Fallback provider in case primary fails
        /// </summary>
        [StringLength(20)]
        public string? FallbackProvider { get; set; }

        /// <summary>
        /// Whether to enable automatic failover
        /// </summary>
        public bool EnableFailover { get; set; } = false;

        /// <summary>
        /// Failover threshold (consecutive failures before switching)
        /// </summary>
        public int FailoverThreshold { get; set; } = 5;

        /// <summary>
        /// Load balancing strategy (RoundRobin, WeightedRandom, Primary)
        /// </summary>
        [StringLength(20)]
        public string LoadBalancingStrategy { get; set; } = "Primary";

        /// <summary>
        /// Priority weight for load balancing (0-100)
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Health check interval in minutes
        /// </summary>
        public int HealthCheckIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Whether health checks are enabled
        /// </summary>
        public bool EnableHealthChecks { get; set; } = true;

        /// <summary>
        /// Last health check timestamp
        /// </summary>
        public DateTime? LastHealthCheck { get; set; }

        /// <summary>
        /// Health status of the configuration
        /// </summary>
        [StringLength(20)]
        public string HealthStatus { get; set; } = "Unknown";

        /// <summary>
        /// Number of consecutive failures
        /// </summary>
        public int ConsecutiveFailures { get; set; } = 0;

        /// <summary>
        /// Last failure timestamp
        /// </summary>
        public DateTime? LastFailure { get; set; }

        /// <summary>
        /// Failure reason
        /// </summary>
        [StringLength(500)]
        public string? LastFailureReason { get; set; }

        /// <summary>
        /// Whether the configuration is currently enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Configuration tags for categorization
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Tags { get; set; }

        /// <summary>
        /// Environment this configuration applies to
        /// </summary>
        [StringLength(20)]
        public string Environment { get; set; } = "Production";
    }

    /// <summary>
    /// Provider health status enumeration
    /// </summary>
    public enum ProviderHealthStatus
    {
        Unknown = 0,
        Healthy = 1,
        Degraded = 2,
        Unhealthy = 3,
        Offline = 4
    }

    /// <summary>
    /// Load balancing strategy enumeration
    /// </summary>
    public enum LoadBalancingStrategy
    {
        Primary = 1,
        RoundRobin = 2,
        WeightedRandom = 3,
        LeastConnections = 4,
        FastestResponse = 5
    }

    /// <summary>
    /// Provider configuration base class
    /// </summary>
    public abstract class ProviderConfiguration
    {
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public Dictionary<string, object>? CustomSettings { get; set; }
    }

    /// <summary>
    /// Twilio provider configuration
    /// </summary>
    public class TwilioProviderConfiguration : ProviderConfiguration
    {
        public string? AccountSid { get; set; }
        public string? AuthToken { get; set; }
        public string? PhoneNumber { get; set; }
        public bool UseSandbox { get; set; } = true;
        public string ApiBaseUrl { get; set; } = "https://api.twilio.com";
        public string ApiVersion { get; set; } = "2010-04-01";
        public int RequestTimeoutSeconds { get; set; } = 30;
        public bool EnableStatusCallbacks { get; set; } = true;
        public string? StatusCallbackUrl { get; set; }
    }

    /// <summary>
    /// Green API provider configuration
    /// </summary>
    public class GreenApiProviderConfiguration : ProviderConfiguration
    {
        public string? InstanceId { get; set; }
        public string? ApiToken { get; set; }
        public string? PhoneNumberId { get; set; }
        public string ApiBaseUrl { get; set; } = "https://api.green-api.com";
        public string ApiVersion { get; set; } = "v3";
        public string? WebhookUrl { get; set; }
        public bool EnableWebhook { get; set; } = true;
        public bool AutoAcknowledgeMessages { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 10;
        public int RequestTimeoutSeconds { get; set; } = 30;
        public bool EnableFileMessages { get; set; } = true;
        public bool EnableLocationMessages { get; set; } = true;
    }

    /// <summary>
    /// Extension methods for WhatsApp provider settings
    /// </summary>
    public static class WhatsAppProviderSettingsExtensions
    {
        /// <summary>
        /// Gets the provider type as enum
        /// </summary>
        public static WhatsAppProviderType GetProviderType(this WhatsAppProviderSettings settings)
        {
            return Enum.TryParse<WhatsAppProviderType>(settings.Provider, true, out var providerType)
                ? providerType
                : WhatsAppProviderType.Twilio;
        }

        /// <summary>
        /// Checks if the configuration is for Twilio
        /// </summary>
        public static bool IsTwilioProvider(this WhatsAppProviderSettings settings)
        {
            return settings.GetProviderType() == WhatsAppProviderType.Twilio;
        }

        /// <summary>
        /// Checks if the configuration is for Green API
        /// </summary>
        public static bool IsGreenApiProvider(this WhatsAppProviderSettings settings)
        {
            return settings.GetProviderType() == WhatsAppProviderType.GreenAPI;
        }

        /// <summary>
        /// Updates the health status
        /// </summary>
        public static void UpdateHealthStatus(this WhatsAppProviderSettings settings, ProviderHealthStatus status, string? reason = null)
        {
            settings.HealthStatus = status.ToString();
            settings.LastHealthCheck = DateTime.UtcNow;

            if (status == ProviderHealthStatus.Unhealthy || status == ProviderHealthStatus.Offline)
            {
                settings.ConsecutiveFailures++;
                settings.LastFailure = DateTime.UtcNow;
                settings.LastFailureReason = reason;
            }
            else
            {
                settings.ConsecutiveFailures = 0;
                settings.LastFailureReason = null;
            }
        }

        /// <summary>
        /// Checks if failover should be triggered
        /// </summary>
        public static bool ShouldTriggerFailover(this WhatsAppProviderSettings settings)
        {
            return settings.EnableFailover &&
                   settings.ConsecutiveFailures >= settings.FailoverThreshold &&
                   !string.IsNullOrEmpty(settings.FallbackProvider);
        }

        /// <summary>
        /// Gets the effective provider (considering failover)
        /// </summary>
        public static string GetEffectiveProvider(this WhatsAppProviderSettings settings)
        {
            if (settings.ShouldTriggerFailover())
            {
                return settings.FallbackProvider ?? settings.Provider;
            }
            return settings.Provider;
        }

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public static List<string> ValidateConfiguration(this WhatsAppProviderSettings settings)
        {
            var errors = new List<string>();

            // Validate provider
            if (!Enum.TryParse<WhatsAppProviderType>(settings.Provider, true, out _))
            {
                errors.Add($"Invalid provider: {settings.Provider}. Must be 'Twilio' or 'GreenAPI'");
            }

            // Validate provider-specific settings
            switch (settings.GetProviderType())
            {
                case WhatsAppProviderType.Twilio:
                    if (string.IsNullOrEmpty(settings.TwilioAccountSid))
                        errors.Add("Twilio Account SID is required");
                    if (string.IsNullOrEmpty(settings.TwilioAuthToken))
                        errors.Add("Twilio Auth Token is required");
                    if (string.IsNullOrEmpty(settings.WhatsAppPhoneNumber))
                        errors.Add("WhatsApp phone number is required");
                    break;

                case WhatsAppProviderType.GreenAPI:
                    if (string.IsNullOrEmpty(settings.GreenApiInstanceId))
                        errors.Add("Green API Instance ID is required");
                    if (string.IsNullOrEmpty(settings.GreenApiToken))
                        errors.Add("Green API Token is required");
                    break;
            }

            // Validate failover settings
            if (settings.EnableFailover && string.IsNullOrEmpty(settings.FallbackProvider))
            {
                errors.Add("Fallback provider must be specified when failover is enabled");
            }

            // Validate intervals
            if (settings.GreenApiPollingIntervalSeconds < 1 || settings.GreenApiPollingIntervalSeconds > 300)
            {
                errors.Add("Green API polling interval must be between 1 and 300 seconds");
            }

            if (settings.HealthCheckIntervalMinutes < 1 || settings.HealthCheckIntervalMinutes > 60)
            {
                errors.Add("Health check interval must be between 1 and 60 minutes");
            }

            return errors;
        }

        /// <summary>
        /// Gets provider-specific configuration
        /// </summary>
        public static T? GetProviderConfig<T>(this WhatsAppProviderSettings settings) where T : ProviderConfiguration
        {
            if (string.IsNullOrEmpty(settings.ProviderConfig))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(settings.ProviderConfig);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sets provider-specific configuration
        /// </summary>
        public static void SetProviderConfig<T>(this WhatsAppProviderSettings settings, T config) where T : ProviderConfiguration
        {
            settings.ProviderConfig = System.Text.Json.JsonSerializer.Serialize(config);
            settings.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// WhatsApp provider type enumeration
    /// </summary>
    public enum WhatsAppProviderType
    {
        Twilio = 1,
        GreenAPI = 2
    }
}