using System;

namespace WebsiteBuilderAPI.Models
{
    public class AvailabilityRule
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? RoomId { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public string RuleValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Room? Room { get; set; }
        public Company Company { get; set; } = null!;
    }
}