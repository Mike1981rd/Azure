using System;

namespace WebsiteBuilderAPI.DTOs
{
    /// <summary>
    /// DTO for returning structural components settings
    /// </summary>
    public class StructuralComponentsDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string HeaderConfig { get; set; } = "{}";
        public string AnnouncementBarConfig { get; set; } = "{}";
        public string FooterConfig { get; set; } = "{}";
        public string ImageBannerConfig { get; set; } = "{}";
        public string CartDrawerConfig { get; set; } = "{}";
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int Version { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating structural components settings
    /// </summary>
    public class CreateStructuralComponentsDto
    {
        public string? HeaderConfig { get; set; }
        public string? AnnouncementBarConfig { get; set; }
        public string? FooterConfig { get; set; }
        public string? ImageBannerConfig { get; set; }
        public string? CartDrawerConfig { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating a specific component
    /// </summary>
    public class UpdateComponentDto
    {
        public string ComponentType { get; set; } = string.Empty; // header, announcementBar, footer, imageBanner, cartDrawer
        public string Config { get; set; } = "{}";
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for publishing components
    /// </summary>
    public class PublishComponentsDto
    {
        public bool PublishAll { get; set; } = true;
        public string[]? ComponentTypes { get; set; } // If not publishing all, specify which ones
        public bool CreateBackup { get; set; } = true;
    }

    /// <summary>
    /// DTO for component preview
    /// </summary>
    public class ComponentPreviewDto
    {
        public string ComponentType { get; set; } = string.Empty;
        public string Config { get; set; } = "{}";
        public string? PageContext { get; set; } // Optional page context for preview
    }
}