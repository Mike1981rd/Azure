using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Unified WhatsApp response model for all providers
    /// </summary>
    public class WhatsAppResponse
    {
        /// <summary>
        /// Unique identifier for the response
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Provider that generated this response
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional response data
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Error details if the operation failed
        /// </summary>
        public WhatsAppError? Error { get; set; }

        /// <summary>
        /// Provider-specific raw response
        /// </summary>
        public object? RawResponse { get; set; }
    }

    /// <summary>
    /// WhatsApp message response model
    /// </summary>
    public class WhatsAppMessageResponse : WhatsAppResponse
    {
        /// <summary>
        /// Message ID from the provider
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Phone number the message was sent to
        /// </summary>
        public string? To { get; set; }

        /// <summary>
        /// Phone number the message was sent from
        /// </summary>
        public string? From { get; set; }

        /// <summary>
        /// Message status
        /// </summary>
        public WhatsAppMessageStatus Status { get; set; } = WhatsAppMessageStatus.Queued;

        /// <summary>
        /// Message content
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Message type
        /// </summary>
        public WhatsAppMessageType MessageType { get; set; } = WhatsAppMessageType.Text;

        /// <summary>
        /// Media URL if applicable
        /// </summary>
        public string? MediaUrl { get; set; }

        /// <summary>
        /// Media content type if applicable
        /// </summary>
        public string? MediaContentType { get; set; }

        /// <summary>
        /// Delivery receipt timestamp
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Read receipt timestamp
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Message cost information
        /// </summary>
        public WhatsAppMessageCost? Cost { get; set; }
    }

    /// <summary>
    /// WhatsApp configuration test response
    /// </summary>
    public class WhatsAppConfigTestResponse : WhatsAppResponse
    {
        /// <summary>
        /// Test message ID
        /// </summary>
        public string? TestMessageId { get; set; }

        /// <summary>
        /// Test phone number used
        /// </summary>
        public string? TestPhoneNumber { get; set; }

        /// <summary>
        /// Configuration validation results
        /// </summary>
        public Dictionary<string, bool>? ValidationResults { get; set; }

        /// <summary>
        /// Connection test duration in milliseconds
        /// </summary>
        public long? ConnectionTestDurationMs { get; set; }

        /// <summary>
        /// API endpoint used for testing
        /// </summary>
        public string? ApiEndpoint { get; set; }
    }

    /// <summary>
    /// WhatsApp webhook response
    /// </summary>
    public class WhatsAppWebhookResponse : WhatsAppResponse
    {
        /// <summary>
        /// Webhook event type
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// Conversation ID
        /// </summary>
        public string? ConversationId { get; set; }

        /// <summary>
        /// Message ID from webhook
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Whether the webhook was processed
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Processing duration in milliseconds
        /// </summary>
        public long ProcessingDurationMs { get; set; }
    }

    /// <summary>
    /// WhatsApp error details
    /// </summary>
    public class WhatsAppError
    {
        /// <summary>
        /// Error code from the provider
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error description
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Error type classification
        /// </summary>
        public WhatsAppErrorType Type { get; set; } = WhatsAppErrorType.Unknown;

        /// <summary>
        /// Whether the error is retryable
        /// </summary>
        public bool IsRetryable { get; set; }

        /// <summary>
        /// Suggested retry delay in seconds
        /// </summary>
        public int? RetryAfterSeconds { get; set; }

        /// <summary>
        /// Additional error context
        /// </summary>
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// WhatsApp message cost information
    /// </summary>
    public class WhatsAppMessageCost
    {
        /// <summary>
        /// Cost amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Pricing model used
        /// </summary>
        public string? PricingModel { get; set; }

        /// <summary>
        /// Additional pricing details
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// WhatsApp message status enumeration
    /// </summary>
    public enum WhatsAppMessageStatus
    {
        /// <summary>
        /// Message queued for sending
        /// </summary>
        Queued = 1,

        /// <summary>
        /// Message sent to provider
        /// </summary>
        Sent = 2,

        /// <summary>
        /// Message delivered to recipient
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// Message read by recipient
        /// </summary>
        Read = 4,

        /// <summary>
        /// Message failed to send
        /// </summary>
        Failed = 5,

        /// <summary>
        /// Message was rejected
        /// </summary>
        Rejected = 6,

        /// <summary>
        /// Message delivery failed
        /// </summary>
        Undelivered = 7,

        /// <summary>
        /// Message status unknown
        /// </summary>
        Unknown = 8
    }

    /// <summary>
    /// WhatsApp message type enumeration
    /// </summary>
    public enum WhatsAppMessageType
    {
        /// <summary>
        /// Text message
        /// </summary>
        Text = 1,

        /// <summary>
        /// Image message
        /// </summary>
        Image = 2,

        /// <summary>
        /// Video message
        /// </summary>
        Video = 3,

        /// <summary>
        /// Audio message
        /// </summary>
        Audio = 4,

        /// <summary>
        /// Document message
        /// </summary>
        Document = 5,

        /// <summary>
        /// Location message
        /// </summary>
        Location = 6,

        /// <summary>
        /// Contact message
        /// </summary>
        Contact = 7,

        /// <summary>
        /// Sticker message
        /// </summary>
        Sticker = 8,

        /// <summary>
        /// Template message
        /// </summary>
        Template = 9,

        /// <summary>
        /// Interactive message (buttons, lists)
        /// </summary>
        Interactive = 10,

        /// <summary>
        /// System message
        /// </summary>
        System = 11,

        /// <summary>
        /// Unknown message type
        /// </summary>
        Unknown = 12
    }

    /// <summary>
    /// WhatsApp error type enumeration
    /// </summary>
    public enum WhatsAppErrorType
    {
        /// <summary>
        /// Authentication error
        /// </summary>
        Authentication = 1,

        /// <summary>
        /// Authorization error
        /// </summary>
        Authorization = 2,

        /// <summary>
        /// Validation error
        /// </summary>
        Validation = 3,

        /// <summary>
        /// Rate limiting error
        /// </summary>
        RateLimit = 4,

        /// <summary>
        /// Network error
        /// </summary>
        Network = 5,

        /// <summary>
        /// Provider API error
        /// </summary>
        ProviderApi = 6,

        /// <summary>
        /// Configuration error
        /// </summary>
        Configuration = 7,

        /// <summary>
        /// Internal server error
        /// </summary>
        Internal = 8,

        /// <summary>
        /// Timeout error
        /// </summary>
        Timeout = 9,

        /// <summary>
        /// Quota exceeded error
        /// </summary>
        QuotaExceeded = 10,

        /// <summary>
        /// Invalid phone number error
        /// </summary>
        InvalidPhoneNumber = 11,

        /// <summary>
        /// Message content error
        /// </summary>
        MessageContent = 12,

        /// <summary>
        /// Media error
        /// </summary>
        Media = 13,

        /// <summary>
        /// Template error
        /// </summary>
        Template = 14,

        /// <summary>
        /// Unknown error type
        /// </summary>
        Unknown = 15
    }

    /// <summary>
    /// Extension methods for WhatsApp responses
    /// </summary>
    public static class WhatsAppResponseExtensions
    {
        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static WhatsAppResponse CreateSuccess(string provider, string message = "Operation completed successfully", object? data = null)
        {
            return new WhatsAppResponse
            {
                Id = Guid.NewGuid().ToString(),
                Success = true,
                Message = message,
                Provider = provider,
                StatusCode = 200,
                Data = data?.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(data) ?? string.Empty)
            };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        public static WhatsAppResponse CreateError(string provider, WhatsAppError error, int statusCode = 500)
        {
            return new WhatsAppResponse
            {
                Id = Guid.NewGuid().ToString(),
                Success = false,
                Message = error.Message,
                Provider = provider,
                StatusCode = statusCode,
                Error = error
            };
        }

        /// <summary>
        /// Creates a message response
        /// </summary>
        public static WhatsAppMessageResponse CreateMessageResponse(
            string provider, 
            string messageId, 
            string to, 
            string from,
            WhatsAppMessageStatus status = WhatsAppMessageStatus.Sent,
            string? content = null)
        {
            return new WhatsAppMessageResponse
            {
                Id = Guid.NewGuid().ToString(),
                Success = true,
                Message = "Message sent successfully",
                Provider = provider,
                StatusCode = 200,
                MessageId = messageId,
                To = to,
                From = from,
                Status = status,
                Content = content
            };
        }

        /// <summary>
        /// Converts a Twilio message status to unified status
        /// </summary>
        public static WhatsAppMessageStatus MapTwilioStatus(string twilioStatus)
        {
            return twilioStatus?.ToLowerInvariant() switch
            {
                "queued" => WhatsAppMessageStatus.Queued,
                "sent" => WhatsAppMessageStatus.Sent,
                "delivered" => WhatsAppMessageStatus.Delivered,
                "read" => WhatsAppMessageStatus.Read,
                "failed" => WhatsAppMessageStatus.Failed,
                "undelivered" => WhatsAppMessageStatus.Undelivered,
                _ => WhatsAppMessageStatus.Unknown
            };
        }

        /// <summary>
        /// Converts a Green API message status to unified status
        /// </summary>
        public static WhatsAppMessageStatus MapGreenApiStatus(string greenApiStatus)
        {
            return greenApiStatus?.ToLowerInvariant() switch
            {
                "sent" => WhatsAppMessageStatus.Sent,
                "delivered" => WhatsAppMessageStatus.Delivered,
                "read" => WhatsAppMessageStatus.Read,
                "failed" => WhatsAppMessageStatus.Failed,
                "error" => WhatsAppMessageStatus.Failed,
                _ => WhatsAppMessageStatus.Unknown
            };
        }

        /// <summary>
        /// Determines if the response indicates a retryable error
        /// </summary>
        public static bool IsRetryable(this WhatsAppResponse response)
        {
            if (response.Success)
                return false;

            return response.Error?.IsRetryable == true ||
                   response.StatusCode >= 500 ||
                   response.Error?.Type == WhatsAppErrorType.Network ||
                   response.Error?.Type == WhatsAppErrorType.Timeout;
        }

        /// <summary>
        /// Gets the suggested retry delay from the response
        /// </summary>
        public static TimeSpan GetRetryDelay(this WhatsAppResponse response)
        {
            if (response.Error?.RetryAfterSeconds.HasValue == true)
            {
                return TimeSpan.FromSeconds(response.Error.RetryAfterSeconds.Value);
            }

            // Default exponential backoff based on error type
            return response.Error?.Type switch
            {
                WhatsAppErrorType.RateLimit => TimeSpan.FromMinutes(1),
                WhatsAppErrorType.QuotaExceeded => TimeSpan.FromHours(1),
                WhatsAppErrorType.Network => TimeSpan.FromSeconds(30),
                WhatsAppErrorType.Timeout => TimeSpan.FromSeconds(10),
                _ => TimeSpan.FromSeconds(5)
            };
        }
    }
}