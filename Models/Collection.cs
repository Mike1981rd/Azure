using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class Collection
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; } // Rich text content
        
        [Required]
        [StringLength(255)]
        public string Handle { get; set; } = string.Empty; // URL slug
        
        [Required]
        public bool IsActive { get; set; } = true; // Estado - Active collections are visible in the store
        
        [StringLength(500)]
        public string? Image { get; set; } // Path to collection image
        
        // Sales Channels
        public bool OnlineStore { get; set; } = true;
        public bool PointOfSale { get; set; } = false;
        public bool Facebook { get; set; } = false;
        public bool Instagram { get; set; } = false;
        public bool TikTok { get; set; } = false;
        public bool WhatsAppBusiness { get; set; } = false;
        
        // SEO Settings
        [StringLength(255)]
        public string? SeoTitle { get; set; }
        
        [StringLength(500)]
        public string? SeoDescription { get; set; }
        
        [StringLength(255)]
        public string? SeoKeywords { get; set; }
        
        // Search engine publication settings
        public bool PublishToSearchEngines { get; set; } = true;
        
        // Sort order for displaying collections
        public int SortOrder { get; set; } = 0;
        
        // Metrics
        public int ProductCount { get; set; } = 0; // Cached count of products
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Soft delete
        
        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<CollectionProduct> CollectionProducts { get; set; }
        
        public Collection()
        {
            CollectionProducts = new HashSet<CollectionProduct>();
            IsActive = true;
            OnlineStore = true;
            PublishToSearchEngines = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Method to generate handle from title
        public static string GenerateHandle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;
                
            // Convert to lowercase, replace spaces with hyphens, remove special characters
            var handle = title.ToLower()
                .Replace(" ", "-")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");
            
            // Remove any character that is not alphanumeric or hyphen
            var cleanHandle = System.Text.RegularExpressions.Regex.Replace(handle, @"[^a-z0-9\-]", "");
            
            // Remove multiple consecutive hyphens
            cleanHandle = System.Text.RegularExpressions.Regex.Replace(cleanHandle, @"-+", "-");
            
            // Trim hyphens from start and end
            return cleanHandle.Trim('-');
        }
    }
}