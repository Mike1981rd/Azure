using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a published snapshot of a website page
    /// Used for serving stable content to production domains with aggressive caching
    /// </summary>
    public class PublishedSnapshot
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Company that owns this snapshot
        /// </summary>
        [Required]
        public int CompanyId { get; set; }
        
        /// <summary>
        /// Associated page ID for tracking
        /// </summary>
        [Required]
        public int PageId { get; set; }
        
        /// <summary>
        /// Page slug for URL routing
        /// </summary>
        [StringLength(255)]
        public string? PageSlug { get; set; }
        
        /// <summary>
        /// Type of page (HOME, PRODUCT, CUSTOM, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PageType { get; set; } = string.Empty;
        
        /// <summary>
        /// Complete snapshot data as JSON
        /// Contains page metadata, sections, theme config, etc.
        /// </summary>
        [Column(TypeName = "jsonb")]
        [Required]
        public string SnapshotData { get; set; } = "{}";
        
        /// <summary>
        /// Version number for tracking changes
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// Indicates if this snapshot is outdated
        /// </summary>
        public bool IsStale { get; set; } = false;
        
        /// <summary>
        /// When this snapshot was published
        /// </summary>
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When this record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
        
        [ForeignKey("PageId")]
        public virtual WebsitePage? Page { get; set; }
    }
}