using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Roles
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}