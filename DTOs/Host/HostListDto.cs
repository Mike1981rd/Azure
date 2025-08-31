using System;

namespace WebsiteBuilderAPI.DTOs.Host
{
    public class HostListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public decimal OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public bool IsSuperhost { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedDate { get; set; }
        public int RoomCount { get; set; }
        public bool IsVerified { get; set; }
    }
}