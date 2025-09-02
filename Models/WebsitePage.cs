using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    /// <summary>
    /// Represents a page in the website builder system
    /// </summary>
    public class WebsitePage
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        /// <summary>
        /// Type of page: HOME, PRODUCT, CART, CHECKOUT, CUSTOM, ALL_COLLECTIONS, COLLECTION, ALL_PRODUCTS
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PageType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Slug { get; set; }
        
        /// <summary>
        /// SEO title for the page
        /// </summary>
        [StringLength(500)]
        public string? MetaTitle { get; set; }
        
        /// <summary>
        /// SEO description for the page
        /// </summary>
        [StringLength(1000)]
        public string? MetaDescription { get; set; }
        
        /// <summary>
        /// Whether the page is active (visible in editor)
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Whether the page is published (visible on live site)
        /// </summary>
        public bool IsPublished { get; set; } = false;
        
        /// <summary>
        /// Date when the page was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }
        
        /// <summary>
        /// Template ID if using a pre-built template
        /// </summary>
        public int? TemplateId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public Company Company { get; set; } = null!;
        public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
        public ICollection<EditorHistory> EditorHistories { get; set; } = new List<EditorHistory>();
    }
    
    /// <summary>
    /// Page type constants
    /// </summary>
    public static class PageTypes
    {
        public const string HOME = "HOME";
        public const string PRODUCT = "PRODUCT";
        public const string CART = "CART";
        public const string CHECKOUT = "CHECKOUT";
        public const string CUSTOM = "CUSTOM";
        public const string ALL_COLLECTIONS = "ALL_COLLECTIONS";
        public const string COLLECTION = "COLLECTION";
        public const string ALL_PRODUCTS = "ALL_PRODUCTS";

        public static readonly HashSet<string> SystemPages = new HashSet<string>
        {
            HOME, CART, CHECKOUT
        };

        public static readonly HashSet<string> AllTypes = new HashSet<string>
        {
            HOME, PRODUCT, CART, CHECKOUT, CUSTOM, ALL_COLLECTIONS, COLLECTION, ALL_PRODUCTS
        };

        public static bool IsSystemPage(string pageType)
        {
            return SystemPages.Contains(pageType);
        }

        public static bool IsValidPageType(string pageType)
        {
            return AllTypes.Contains(pageType);
        }
    }
}