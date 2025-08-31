using System;

namespace WebsiteBuilderAPI.Models
{
    public class RoomBlockPeriod
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsRecurring { get; set; } = false;
        public string? RecurrencePattern { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public Room? Room { get; set; }
        public Company Company { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
    }
}