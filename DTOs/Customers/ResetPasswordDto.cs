using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class ResetPasswordDto
    {
        // Admin can optionally set a custom temporary password
        public string? TemporaryPassword { get; set; }
        
        // Force user to change password on next login
        public bool ForcePasswordChange { get; set; } = true;
        
        // Send email notification to customer
        public bool SendEmail { get; set; } = true;
    }
    
    public class ResetPasswordResponseDto
    {
        public bool Success { get; set; }
        public string? TemporaryPassword { get; set; }
        public string Message { get; set; }
        public bool EmailSent { get; set; }
    }
}