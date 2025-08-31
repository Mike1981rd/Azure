using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Paginas
{
    public class CreatePaginaDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(255, ErrorMessage = "El título no puede exceder 255 caracteres")]
        [MinLength(1, ErrorMessage = "El título debe tener al menos 1 caracter")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El slug es obligatorio")]
        [MaxLength(255, ErrorMessage = "El slug no puede exceder 255 caracteres")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", 
            ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones. Ejemplo: 'sobre-nosotros'")]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(50000, ErrorMessage = "El contenido no puede exceder 50,000 caracteres")]
        public string? Content { get; set; }

        public bool IsVisible { get; set; } = true;

        [RegularExpression(@"^(draft|published|scheduled)$", 
            ErrorMessage = "El estado de publicación debe ser: draft, published o scheduled")]
        public string PublishStatus { get; set; } = "draft";

        public DateTime? PublishedAt { get; set; }
        public DateTime? ScheduledPublishAt { get; set; }

        [MaxLength(50, ErrorMessage = "El template no puede exceder 50 caracteres")]
        public string Template { get; set; } = "default";

        // SEO
        [MaxLength(255, ErrorMessage = "El meta título no puede exceder 255 caracteres")]
        public string? MetaTitle { get; set; }

        [MaxLength(500, ErrorMessage = "La meta descripción no puede exceder 500 caracteres")]
        public string? MetaDescription { get; set; }

        [MaxLength(500, ErrorMessage = "Las palabras clave no pueden exceder 500 caracteres")]
        public string? MetaKeywords { get; set; }

        [Url(ErrorMessage = "La imagen OG debe ser una URL válida")]
        [MaxLength(500, ErrorMessage = "La URL de imagen OG no puede exceder 500 caracteres")]
        public string? OgImage { get; set; }

        [MaxLength(255, ErrorMessage = "El título OG no puede exceder 255 caracteres")]
        public string? OgTitle { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción OG no puede exceder 500 caracteres")]
        public string? OgDescription { get; set; }

        public bool AllowSearchEngines { get; set; } = true;

        [Url(ErrorMessage = "La URL canónica debe ser una URL válida")]
        [MaxLength(500, ErrorMessage = "La URL canónica no puede exceder 500 caracteres")]
        public string? CanonicalUrl { get; set; }

        [RegularExpression(@"^(index,follow|index,nofollow|noindex,follow|noindex,nofollow)$",
            ErrorMessage = "El valor de robots debe ser uno de: index,follow | index,nofollow | noindex,follow | noindex,nofollow")]
        public string? Robots { get; set; }

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validar fechas de publicación
            if (PublishStatus == "scheduled" && !ScheduledPublishAt.HasValue)
            {
                yield return new ValidationResult(
                    "La fecha de publicación programada es obligatoria cuando el estado es 'scheduled'",
                    new[] { nameof(ScheduledPublishAt) });
            }

            if (PublishStatus == "published" && !PublishedAt.HasValue)
            {
                yield return new ValidationResult(
                    "La fecha de publicación es obligatoria cuando el estado es 'published'",
                    new[] { nameof(PublishedAt) });
            }

            if (ScheduledPublishAt.HasValue && ScheduledPublishAt <= DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    "La fecha de publicación programada debe ser futura",
                    new[] { nameof(ScheduledPublishAt) });
            }
        }
    }
}