using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.ConfigOptions
{
    public class CreateConfigOptionDto
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string LabelEs { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string LabelEn { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Icon { get; set; }
        
        [StringLength(50)]
        public string? IconType { get; set; } = "heroicon";
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        public int SortOrder { get; set; } = 999;
        public bool IsActive { get; set; } = true;
    }
}