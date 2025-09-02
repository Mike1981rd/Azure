using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Host
{
    public class UpdateHostDto
    {
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [StringLength(100)]
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string ProfilePicture { get; set; }
        
        [StringLength(1000)]
        public string Bio { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        // Extended Information
        public int? YearStartedHosting { get; set; }
        
        [StringLength(1000)]
        public string AboutMe { get; set; }
        
        [StringLength(200)]
        public string Location { get; set; }
        
        [StringLength(100)]
        public string Work { get; set; }
        
        public List<string> Attributes { get; set; }
        public List<string> Hobbies { get; set; }
        public List<string> Languages { get; set; }
        
        public bool? IsPhoneVerified { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsIdentityVerified { get; set; }
        
        public int? ResponseTimeMinutes { get; set; }
        
        public bool? IsSuperhost { get; set; }
        public bool? IsActive { get; set; }
    }
}