using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a section within a website page
    /// </summary>
    public class PageSection
    {
        public int Id { get; set; }
        
        [Required]
        public int PageId { get; set; }
        
        /// <summary>
        /// Type of section: ImageWithText, Gallery, RichText, etc.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SectionType { get; set; } = string.Empty;
        
        /// <summary>
        /// Section-specific configuration stored as JSONB
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string Config { get; set; } = "{}";
        
        /// <summary>
        /// Theme overrides for this specific section (JSONB)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? ThemeOverrides { get; set; }
        
        /// <summary>
        /// Order of the section within the page
        /// </summary>
        public int SortOrder { get; set; }
        
        /// <summary>
        /// Whether the section is visible
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional custom CSS class for the section
        /// </summary>
        [StringLength(255)]
        public string? CustomCssClass { get; set; }
        
        /// <summary>
        /// Optional section title for editor reference
        /// </summary>
        [StringLength(255)]
        public string? Title { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("PageId")]
        public WebsitePage Page { get; set; } = null!;
        
        /// <summary>
        /// Child blocks for sections that support them (e.g., PRODUCT_INFORMATION)
        /// </summary>
        public ICollection<PageSectionChild> Children { get; set; } = new List<PageSectionChild>();
    }
    
    /// <summary>
    /// Section type constants
    /// </summary>
    public static class SectionTypes
    {
        // Page-specific sections (Phase 2 - Priority)
        public const string PRODUCTS = "Products";              // For collection/all-products pages
        public const string COLLECTIONS_LIST = "CollectionsList"; // For all-collections page
        public const string CART = "Cart";                      // For cart page
        public const string PRODUCT_INFORMATION = "ProductInformation"; // For product page
        
        // Modular reusable sections (Phase 3)
        public const string IMAGE_WITH_TEXT = "ImageWithText";
        public const string IMAGE_BANNER = "ImageBanner";
        public const string RICH_TEXT = "RichText";
        public const string GALLERY = "Gallery";
        public const string CONTACT_FORM = "ContactForm";
        public const string NEWSLETTER = "Newsletter";
        public const string FEATURED_PRODUCT = "FeaturedProduct";
        public const string FEATURED_COLLECTION = "FeaturedCollection";
        public const string TESTIMONIALS = "Testimonials";
        public const string FAQ = "FAQ";
        public const string VIDEOS = "Videos";
        public const string SLIDESHOW = "Slideshow";
        
        /// <summary>
        /// Page-specific sections that are usually automatically added
        /// </summary>
        public static readonly HashSet<string> PageSpecificTypes = new HashSet<string>
        {
            PRODUCTS, COLLECTIONS_LIST, CART, PRODUCT_INFORMATION
        };
        
        /// <summary>
        /// Modular sections that can be added to any page
        /// </summary>
        public static readonly HashSet<string> ModularTypes = new HashSet<string>
        {
            IMAGE_WITH_TEXT, IMAGE_BANNER, RICH_TEXT, GALLERY,
            CONTACT_FORM, NEWSLETTER, FEATURED_PRODUCT,
            FEATURED_COLLECTION, TESTIMONIALS, FAQ, VIDEOS, SLIDESHOW
        };
        
        /// <summary>
        /// All valid section types
        /// </summary>
        public static readonly HashSet<string> AllTypes = new HashSet<string>
        {
            // Page-specific
            PRODUCTS, COLLECTIONS_LIST, CART, PRODUCT_INFORMATION,
            // Modular
            IMAGE_WITH_TEXT, IMAGE_BANNER, RICH_TEXT, GALLERY,
            CONTACT_FORM, NEWSLETTER, FEATURED_PRODUCT,
            FEATURED_COLLECTION, TESTIMONIALS, FAQ, VIDEOS, SLIDESHOW
        };
        
        /// <summary>
        /// Sections that support child blocks
        /// </summary>
        public static readonly HashSet<string> SectionsWithChildren = new HashSet<string>
        {
            PRODUCT_INFORMATION,
            SLIDESHOW
        };
        
        public static bool IsValidSectionType(string sectionType)
        {
            return AllTypes.Contains(sectionType);
        }
        
        public static bool IsPageSpecific(string sectionType)
        {
            return PageSpecificTypes.Contains(sectionType);
        }
        
        public static bool IsModular(string sectionType)
        {
            return ModularTypes.Contains(sectionType);
        }
        
        public static bool SupportsChildren(string sectionType)
        {
            return SectionsWithChildren.Contains(sectionType);
        }
    }
}