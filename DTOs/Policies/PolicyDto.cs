using System;

namespace WebsiteBuilderAPI.DTOs.Policies
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Additional properties for UI
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}