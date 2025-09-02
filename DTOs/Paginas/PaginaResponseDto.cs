namespace WebsiteBuilderAPI.DTOs.Paginas
{
    public class PaginaResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        // Información básica
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string Url => $"/{Slug}"; // URL completa de la página
        
        // Visibilidad y estado
        public bool IsVisible { get; set; }
        public string PublishStatus { get; set; } = string.Empty;
        public string PublishStatusDisplay => PublishStatus switch
        {
            "draft" => "Borrador",
            "published" => "Publicado",
            "scheduled" => "Programado",
            _ => "Desconocido"
        };
        public DateTime? PublishedAt { get; set; }
        public DateTime? ScheduledPublishAt { get; set; }
        
        // Template
        public string Template { get; set; } = string.Empty;
        public string TemplateDisplay => Template switch
        {
            "default" => "Predeterminado",
            "fullwidth" => "Ancho completo",
            "sidebar" => "Con barra lateral",
            _ => Template
        };
        
        // SEO y Metacampos
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? OgImage { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        
        // Control de motores de búsqueda
        public bool AllowSearchEngines { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? Robots { get; set; }
        
        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        
        // Información adicional
        public int ContentLength => Content?.Length ?? 0;
        public bool HasSeoData => !string.IsNullOrEmpty(MetaTitle) || 
                                  !string.IsNullOrEmpty(MetaDescription) || 
                                  !string.IsNullOrEmpty(MetaKeywords);
        public bool IsScheduledForFuture => PublishStatus == "scheduled" && 
                                           ScheduledPublishAt.HasValue && 
                                           ScheduledPublishAt > DateTime.UtcNow;
    }
}