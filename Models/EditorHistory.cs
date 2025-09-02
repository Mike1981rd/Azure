using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// History tracking for editor changes (undo/redo functionality)
    /// </summary>
    public class EditorHistory
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Type of entity being tracked (page, component, section)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = "page";
        
        /// <summary>
        /// ID of the entity being tracked
        /// </summary>
        [Required]
        public int EntityId { get; set; }
        
        /// <summary>
        /// For page-specific history (optional)
        /// </summary>
        public int? PageId { get; set; }
        
        /// <summary>
        /// Company ID for isolation
        /// </summary>
        [Required]
        public int CompanyId { get; set; }
        
        /// <summary>
        /// User who made the change
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// Complete state data as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string StateData { get; set; } = "{}";
        
        /// <summary>
        /// Change type (create, update, delete, reorder, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ChangeType { get; set; } = "update";
        
        /// <summary>
        /// Human-readable description of the change
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Version number for this state
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// Whether this is a checkpoint (major save point)
        /// </summary>
        public bool IsCheckpoint { get; set; } = false;
        
        /// <summary>
        /// Session ID to group related changes
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Expiration time for auto-cleanup (null = never expire)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("PageId")]
        public WebsitePage? Page { get; set; }
        
        [ForeignKey("CompanyId")]
        public Company Company { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
    
    /// <summary>
    /// Constants for editor history
    /// </summary>
    public static class EditorHistoryConstants
    {
        // Entity types
        public const string ENTITY_PAGE = "page";
        public const string ENTITY_SECTION = "section";
        public const string ENTITY_COMPONENT = "component";
        public const string ENTITY_THEME = "theme";
        
        // Change types
        public const string CHANGE_CREATE = "create";
        public const string CHANGE_UPDATE = "update";
        public const string CHANGE_DELETE = "delete";
        public const string CHANGE_REORDER = "reorder";
        public const string CHANGE_PUBLISH = "publish";
        public const string CHANGE_UNPUBLISH = "unpublish";
        public const string CHANGE_DUPLICATE = "duplicate";
        
        // Default settings
        public const int MAX_HISTORY_ITEMS = 50;
        public const int HISTORY_EXPIRATION_DAYS = 30;
    }
}