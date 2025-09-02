using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerDevice
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Browser { get; set; } // Chrome/Firefox/Safari/Edge
        
        [Required]
        [StringLength(50)]
        public string DeviceType { get; set; } // Desktop/Mobile/Tablet
        
        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; } // "HP Spectre 360", "iPhone 12x"
        
        [Required]
        [StringLength(50)]
        public string OperatingSystem { get; set; } // Windows/macOS/iOS/Android/Linux
        
        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Location { get; set; } // "Switzerland", "Australia"
        
        public DateTime LastActivity { get; set; }
        public DateTime FirstSeen { get; set; }
        
        public bool IsTrusted { get; set; }
        public bool IsActive { get; set; }
        
        // User agent string for detailed info
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        // Session tracking
        public string? SessionId { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        
        public CustomerDevice()
        {
            FirstSeen = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
            IsActive = true;
            IsTrusted = false;
        }
        
        // Helper method to format activity time
        public string GetFormattedLastActivity()
        {
            var timeDiff = DateTime.UtcNow - LastActivity;
            
            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes} minutes ago";
            if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours} hours ago";
            if (timeDiff.TotalDays < 30)
                return $"{(int)timeDiff.TotalDays} days ago";
            
            return LastActivity.ToString("MMM dd, yyyy");
        }
    }
}