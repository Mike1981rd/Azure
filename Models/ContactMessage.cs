using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a contact message from website visitors
    /// </summary>
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Company that receives this message
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Sender's name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sender's email
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Sender's phone (optional)
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Message subject
        /// </summary>
        [StringLength(200)]
        public string? Subject { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Source of the message (website, whatsapp, email, etc.)
        /// </summary>
        [StringLength(50)]
        public string Source { get; set; } = "website";

        /// <summary>
        /// Current status (unread, read, archived)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "unread";

        /// <summary>
        /// Whether notification was sent to admin
        /// </summary>
        public bool IsNotificationSent { get; set; } = false;

        /// <summary>
        /// IP address of the sender
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent of the sender's browser
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the message was read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// When the message was archived
        /// </summary>
        public DateTime? ArchivedAt { get; set; }

        /// <summary>
        /// User who read the message
        /// </summary>
        public int? ReadByUserId { get; set; }

        /// <summary>
        /// Any internal notes about this message
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual User? ReadByUser { get; set; }
    }
}