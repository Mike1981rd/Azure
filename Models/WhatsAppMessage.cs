using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a WhatsApp message in the system
    /// </summary>
    public class WhatsAppMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Twilio Message SID - Unique identifier from Twilio
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TwilioSid { get; set; } = string.Empty;

        /// <summary>
        /// Phone number that sent the message (with country code)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Phone number that received the message (business number)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Message content/body
        /// </summary>
        [StringLength(4096)]
        public string? Body { get; set; }

        /// <summary>
        /// Message type (text, image, audio, video, document, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// Media URL if message contains media
        /// </summary>
        [StringLength(500)]
        public string? MediaUrl { get; set; }

        /// <summary>
        /// Media content type (image/jpeg, audio/ogg, etc.)
        /// </summary>
        [StringLength(50)]
        public string? MediaContentType { get; set; }

        /// <summary>
        /// Direction of message (inbound/outbound)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Direction { get; set; } = "inbound";

        /// <summary>
        /// Current status of the message (sent, delivered, read, failed, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "received";

        /// <summary>
        /// Error code if message failed
        /// </summary>
        [StringLength(10)]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Conversation this message belongs to
        /// </summary>
        [Required]
        public Guid ConversationId { get; set; }

        /// <summary>
        /// Company that owns this message
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Customer ID if message is from a registered customer
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// User who replied (for outbound messages)
        /// </summary>
        public int? RepliedByUserId { get; set; }

        /// <summary>
        /// Timestamp when message was sent/received
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When message was delivered (for outbound)
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// When message was read (for outbound)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Additional metadata in JSON format
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Source of the message: whatsapp, widget, api
        /// </summary>
        [StringLength(20)]
        public string Source { get; set; } = "whatsapp";

        /// <summary>
        /// Session ID for widget conversations
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// When record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft delete flag - message appears as deleted in UI
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// When the message was deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User who deleted the message
        /// </summary>
        public int? DeletedByUserId { get; set; }

        // Navigation properties
        public virtual WhatsAppConversation Conversation { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
        public virtual User? RepliedByUser { get; set; }
    }
}
