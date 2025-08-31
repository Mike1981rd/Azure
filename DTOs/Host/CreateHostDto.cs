using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Host
{
    public class CreateHostDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string ProfilePicture { get; set; }
        
        [StringLength(1000)]
        public string Bio { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        public List<string> Languages { get; set; }
        
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsIdentityVerified { get; set; }
        
        public int ResponseTimeMinutes { get; set; } = 60;
        
        public bool IsSuperhost { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        public int CompanyId { get; set; }
    }
}