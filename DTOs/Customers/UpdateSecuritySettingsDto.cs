using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class UpdateSecuritySettingsDto
    {
        // Password change (optional - only if changing password)
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        
        // Two-Factor Authentication
        public bool? TwoFactorEnabled { get; set; }
        public string? TwoFactorPhone { get; set; }
        
        // Recovery settings
        [EmailAddress]
        public string? RecoveryEmail { get; set; }
        
        // Session settings
        [Range(5, 1440)] // 5 minutes to 24 hours
        public int? SessionTimeoutMinutes { get; set; }
        
        // Security questions
        public List<SecurityQuestionDto>? SecurityQuestions { get; set; }
    }
    
    public class SecurityQuestionDto
    {
        public int? Id { get; set; } // For existing questions
        [Required]
        public string Question { get; set; }
        [Required]
        public string Answer { get; set; } // Will be hashed before saving
        public bool? Delete { get; set; } // Mark for deletion
    }
    
    public class CustomerPasswordChangeDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
    
    public class CustomerSecurityQuestionDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Question { get; set; }
        // Never return the answer hash to frontend
    }
}