using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a child block within a PageSection (e.g., blocks inside PRODUCT_INFORMATION)
    /// </summary>
    public class PageSectionChild
    {
        public int Id { get; set; }
        
        [Required]
        public int PageSectionId { get; set; }
        
        /// <summary>
        /// Type of child block (e.g., title, price, buyButtons, variantPicker)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BlockType { get; set; } = string.Empty;
        
        /// <summary>
        /// Block-specific settings stored as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string Settings { get; set; } = "{}";
        
        /// <summary>
        /// Order of the block within its parent section
        /// </summary>
        public int SortOrder { get; set; }
        
        /// <summary>
        /// Whether the block is visible
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional custom CSS class for the block
        /// </summary>
        [StringLength(255)]
        public string? CustomCssClass { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PageSectionId")]
        public PageSection PageSection { get; set; } = null!;
    }
    
    /// <summary>
    /// Product Information block types
    /// </summary>
    public static class ProductInfoBlockTypes
    {
        public const string TITLE = "title";
        public const string PRICE = "price";
        public const string INVENTORY_STATUS = "inventoryStatus";
        public const string STAR_RATINGS = "starRatings";
        public const string CONTENT_TABS = "contentTabs";
        public const string DESCRIPTION = "description";
        public const string BUY_BUTTONS = "buyButtons";
        public const string VARIANT_PICKER = "variantPicker";
        public const string QUANTITY_SELECTOR = "quantitySelector";
        public const string VENDOR = "vendor";
        public const string SKU = "sku";
        public const string SHARE_BUTTONS = "shareButtons";
        public const string PICKUP_AVAILABILITY = "pickupAvailability";
        public const string GIFT_CARD_RECIPIENT = "giftCardRecipient";
        public const string CUSTOM_LIQUID = "customLiquid";
        public const string COLLAPSIBLE_TAB = "collapsibleTab";
        public const string POPUP = "popup";
        public const string TEXT = "text";
        public const string CUSTOM_HTML = "customHtml";
        
        public static readonly HashSet<string> AllTypes = new HashSet<string>
        {
            TITLE, PRICE, INVENTORY_STATUS, STAR_RATINGS, CONTENT_TABS,
            DESCRIPTION, BUY_BUTTONS, VARIANT_PICKER, QUANTITY_SELECTOR,
            VENDOR, SKU, SHARE_BUTTONS, PICKUP_AVAILABILITY,
            GIFT_CARD_RECIPIENT, CUSTOM_LIQUID, COLLAPSIBLE_TAB,
            POPUP, TEXT, CUSTOM_HTML
        };
        
        /// <summary>
        /// Default blocks for a new PRODUCT_INFORMATION section
        /// </summary>
        public static readonly List<(string type, int order)> DefaultBlocks = new List<(string, int)>
        {
            (TITLE, 1),
            (PRICE, 2),
            (INVENTORY_STATUS, 3),
            (VARIANT_PICKER, 4),
            (QUANTITY_SELECTOR, 5),
            (BUY_BUTTONS, 6),
            (DESCRIPTION, 7),
            (SHARE_BUTTONS, 8)
        };
        
        public static bool IsValidBlockType(string blockType)
        {
            return AllTypes.Contains(blockType);
        }
    }
}