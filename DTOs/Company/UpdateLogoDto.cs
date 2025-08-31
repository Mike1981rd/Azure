using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Company
{
    public class UpdateLogoDto
    {
        [Required]
        [StringLength(500)]
        public string Logo { get; set; } = string.Empty;
    }
}