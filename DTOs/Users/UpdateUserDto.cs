using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }
        
        // Password is optional for updates
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [MinLength(1, ErrorMessage = "At least one role is required")]
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}