using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models.Components
{
    /// <summary>
    /// Settings for structural components (Header, Footer, Announcement Bar, Cart Drawer)
    /// </summary>
    public class StructuralComponentsSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Header configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string HeaderConfig { get; set; } = "{}";

        /// <summary>
        /// Announcement bar configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string AnnouncementBarConfig { get; set; } = "{}";

        /// <summary>
        /// Footer configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string FooterConfig { get; set; } = "{}";

        /// <summary>
        /// Image banner configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string ImageBannerConfig { get; set; } = "{}";

        /// <summary>
        /// Cart drawer configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string CartDrawerConfig { get; set; } = "{}";

        /// <summary>
        /// WhatsApp widget configuration as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string WhatsAppWidgetConfig { get; set; } = "{}";

        /// <summary>
        /// Whether this configuration is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this configuration is published to the live site
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// Date when the configuration was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Version number for tracking changes
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Notes or description for this configuration
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who last updated this configuration
        /// </summary>
        public int? LastUpdatedBy { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("LastUpdatedBy")]
        public virtual User? LastUpdatedByUser { get; set; }
    }
}