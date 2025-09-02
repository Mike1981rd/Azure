using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// DTO for messages sent from the website widget
    /// </summary>
    public class WidgetMessageDto
    {
        /// <summary>
        /// The message content
        /// </summary>
        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Customer's name (if provided)
        /// </summary>
        [StringLength(100)]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Customer's email (if provided)
        /// </summary>
        [EmailAddress]
        [StringLength(255)]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Customer's phone (if provided)
        /// </summary>
        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// The page URL where the widget was opened
        /// </summary>
        [StringLength(500)]
        public string? PageUrl { get; set; }

        /// <summary>
        /// Session ID to track the conversation
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// User agent of the browser
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// IP address of the visitor
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Optional client-generated id to ensure idempotency on server
        /// </summary>
        [StringLength(100)]
        public string? ClientMessageId { get; set; }
    }

    /// <summary>
    /// DTO for widget conversation status updates
    /// </summary>
    public class WidgetConversationStatusDto
    {
        /// <summary>
        /// Session ID of the widget conversation
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// New status: active, closed, resolved
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Optional closing message
        /// </summary>
        [StringLength(500)]
        public string? ClosingMessage { get; set; }
    }

    /// <summary>
    /// DTO for sending responses back to the widget
    /// </summary>
    public class WidgetResponseDto
    {
        /// <summary>
        /// The response message
        /// </summary>
        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Agent/User name sending the response
        /// </summary>
        [StringLength(100)]
        public string? AgentName { get; set; }

        /// <summary>
        /// Message type: text, system, closing
        /// </summary>
        [StringLength(20)]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// Optional client-generated id to ensure idempotency
        /// </summary>
        [StringLength(100)]
        public string? ClientMessageId { get; set; }

        /// <summary>
        /// Timestamp of the message
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional session id to bind/update the conversation's active widget session
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }
    }
}
