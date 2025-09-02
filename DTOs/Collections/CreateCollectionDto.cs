using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Collections
{
    public class CreateCollectionDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [StringLength(255, ErrorMessage = "Handle cannot exceed 255 characters")]
        public string? Handle { get; set; } // If not provided, will be auto-generated from title
        
        public bool? IsActive { get; set; }
        
        public string? Image { get; set; }
        
        // Sales Channels - all optional with defaults in backend
        public bool? OnlineStore { get; set; }
        public bool? PointOfSale { get; set; }
        public bool? Facebook { get; set; }
        public bool? Instagram { get; set; }
        public bool? TikTok { get; set; }
        public bool? WhatsAppBusiness { get; set; }
        
        // SEO Settings
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public bool? PublishToSearchEngines { get; set; }
        
        // Products to add to collection
        public List<int>? ProductIds { get; set; }
    }
}