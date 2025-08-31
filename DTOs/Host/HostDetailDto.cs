using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Host
{
    public class HostDetailDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        
        // Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        
        // Extended Information
        public int? YearStartedHosting { get; set; }
        public string AboutMe { get; set; }
        public string Location { get; set; }
        public string Work { get; set; }
        public List<string> Attributes { get; set; }
        public List<string> Hobbies { get; set; }
        
        // Verification
        public bool IsVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsIdentityVerified { get; set; }
        public List<string> Languages { get; set; }
        
        // Metrics
        public decimal OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public decimal AcceptanceRate { get; set; }
        public bool IsSuperhost { get; set; }
        
        // Configuration
        public bool IsActive { get; set; }
        public DateTime? LastActiveDate { get; set; }
        
        // Related data
        public int RoomCount { get; set; }
        public int ActiveRoomCount { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}