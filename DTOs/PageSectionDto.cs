using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs
{
    /// <summary>
    /// DTO for returning page section information
    /// </summary>
    public class PageSectionDto
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string SectionType { get; set; } = string.Empty;
        public string Config { get; set; } = "{}";
        public string? ThemeOverrides { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? CustomCssClass { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PageSectionChildDto> Children { get; set; } = new List<PageSectionChildDto>();
    }

    /// <summary>
    /// DTO for creating a new page section
    /// </summary>
    public class CreatePageSectionDto
    {
        public string SectionType { get; set; } = string.Empty;
        public string? Config { get; set; }
        public string? ThemeOverrides { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public string? CustomCssClass { get; set; }
        public string? Title { get; set; }
    }

    /// <summary>
    /// DTO for updating a page section
    /// </summary>
    public class UpdatePageSectionDto
    {
        public string? Config { get; set; }
        public string? ThemeOverrides { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public string? CustomCssClass { get; set; }
        public string? Title { get; set; }
    }

    /// <summary>
    /// DTO for reordering sections
    /// </summary>
    public class ReorderSectionsDto
    {
        public List<SectionOrderDto> SectionOrders { get; set; } = new List<SectionOrderDto>();
    }

    /// <summary>
    /// DTO for section ordering
    /// </summary>
    public class SectionOrderDto
    {
        public int SectionId { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// DTO for duplicating a section
    /// </summary>
    public class DuplicateSectionDto
    {
        public int? TargetPageId { get; set; }
        public int? SortOrder { get; set; }
        public string? NewTitle { get; set; }
    }

    /// <summary>
    /// DTO for page section child block
    /// </summary>
    public class PageSectionChildDto
    {
        public int Id { get; set; }
        public int PageSectionId { get; set; }
        public string BlockType { get; set; } = string.Empty;
        public string Settings { get; set; } = "{}";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? CustomCssClass { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}