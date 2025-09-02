using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a WhatsApp conversation thread with a customer
    /// </summary>
    public class WhatsAppConversation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Customer's phone number (with country code)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Customer's display name (from WhatsApp profile or manual entry)
        /// </summary>
        [StringLength(100)]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Customer's email (for widget conversations)
        /// </summary>
        [StringLength(255)]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Source of the conversation: whatsapp, widget
        /// </summary>
        [StringLength(20)]
        public string Source { get; set; } = "whatsapp";

        /// <summary>
        /// Session ID for widget conversations
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// Business WhatsApp number
        /// </summary>
        [Required]
        [StringLength(20)]
        public string BusinessPhone { get; set; } = string.Empty;

        /// <summary>
        /// Current status of conversation (active, closed, archived, blocked)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// Priority level (low, normal, high, urgent)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// Assigned user/agent for this conversation
        /// </summary>
        public int? AssignedUserId { get; set; }

        /// <summary>
        /// Company that owns this conversation
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Customer ID if registered in the system
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Number of unread messages from customer
        /// </summary>
        public int UnreadCount { get; set; } = 0;

        /// <summary>
        /// Total number of messages in conversation
        /// </summary>
        public int MessageCount { get; set; } = 0;

        /// <summary>
        /// Last message content preview
        /// </summary>
        [StringLength(200)]
        public string? LastMessagePreview { get; set; }

        /// <summary>
        /// Last message timestamp
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Who sent the last message (customer/business)
        /// </summary>
        [StringLength(10)]
        public string? LastMessageSender { get; set; }

        /// <summary>
        /// Tags for categorization (JSON array)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Tags { get; set; }

        /// <summary>
        /// Internal notes about the conversation
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Customer profile data from WhatsApp
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? CustomerProfile { get; set; }

        /// <summary>
        /// Additional conversation metadata
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }

        /// <summary>
        /// When conversation was started
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When conversation was closed
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// When conversation was archived
        /// </summary>
        public DateTime? ArchivedAt { get; set; }

        /// <summary>
        /// When record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
        public virtual User? AssignedUser { get; set; }
        public virtual ICollection<WhatsAppMessage> Messages { get; set; } = new List<WhatsAppMessage>();
    }
}