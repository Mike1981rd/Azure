using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.ConfigOptions
{
    public class UpdateConfigOptionDto
    {
        [StringLength(200)]
        public string? LabelEs { get; set; }
        
        [StringLength(200)]
        public string? LabelEn { get; set; }
        
        [StringLength(100)]
        public string? Icon { get; set; }
        
        [StringLength(50)]
        public string? IconType { get; set; }
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}