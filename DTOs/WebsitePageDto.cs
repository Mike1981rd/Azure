using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs
{
    /// <summary>
    /// DTO for returning website page information
    /// </summary>
    public class WebsitePageDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PageType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? TemplateId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PageSectionDto> Sections { get; set; } = new List<PageSectionDto>();
    }

    /// <summary>
    /// DTO for creating a new website page
    /// </summary>
    public class CreateWebsitePageDto
    {
        public string PageType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public int? TemplateId { get; set; }
    }

    /// <summary>
    /// DTO for updating a website page
    /// </summary>
    public class UpdateWebsitePageDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for publishing a website page
    /// </summary>
    public class PublishWebsitePageDto
    {
        public bool CreateBackup { get; set; } = true;
        public string? PublishNotes { get; set; }
    }

    /// <summary>
    /// Request payload for replacing all sections of a page (bulk update from editor)
    /// </summary>
    public class UpdatePageSectionsDto
    {
        public List<EditorSectionInputDto> Sections { get; set; } = new List<EditorSectionInputDto>();
    }

    /// <summary>
    /// Editor-side section shape used for bulk updates
    /// </summary>
    public class EditorSectionInputDto
    {
        public string Type { get; set; } = string.Empty; // e.g., image_banner, image_with_text, rich_text, etc.
        public int SortOrder { get; set; }
        public bool Visible { get; set; } = true;
        public string? Name { get; set; }
        public object? Settings { get; set; } // Arbitrary JSON from editor
    }
}