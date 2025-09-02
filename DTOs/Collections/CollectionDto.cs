using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Collections
{
    public class CollectionDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Handle { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Image { get; set; }
        
        // Sales Channels
        public bool OnlineStore { get; set; }
        public bool PointOfSale { get; set; }
        public bool Facebook { get; set; }
        public bool Instagram { get; set; }
        public bool TikTok { get; set; }
        public bool WhatsAppBusiness { get; set; }
        
        // SEO Settings
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public bool PublishToSearchEngines { get; set; }
        
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        
        // Conditions summary for UI
        public string ConditionsSummary { get; set; } = "Todas las condiciones";
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Products in collection (optional, for detail view)
        public List<CollectionProductDto>? Products { get; set; }
    }
    
    public class CollectionProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public List<string>? Images { get; set; }
        public int Stock { get; set; }
        public string? Sku { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class CollectionListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string? Image { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public string ConditionsSummary { get; set; } = "Todas las condiciones";
        public DateTime UpdatedAt { get; set; }
    }
}