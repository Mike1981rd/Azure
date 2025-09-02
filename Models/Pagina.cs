using System;

namespace WebsiteBuilderAPI.Models
{
    public class Pagina
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        // Información básica
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Content { get; set; } // HTML del rich text editor
        
        // Visibilidad y estado
        public bool IsVisible { get; set; } = true;
        public string PublishStatus { get; set; } = "draft"; // draft, published, scheduled
        public DateTime? PublishedAt { get; set; }
        public DateTime? ScheduledPublishAt { get; set; }
        
        // Template
        public string Template { get; set; } = "default"; // default, fullwidth, sidebar, etc.
        
        // SEO y Metacampos
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? OgImage { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        
        // Control de motores de búsqueda
        public bool AllowSearchEngines { get; set; } = true;
        public string? CanonicalUrl { get; set; }
        public string? Robots { get; set; } // index,follow / noindex,nofollow
        
        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        
        // Navegación
        public Company Company { get; set; } = null!;
        public User? CreatedBy { get; set; }
        public User? UpdatedBy { get; set; }
    }
}