using System;

namespace WebsiteBuilderAPI.Models
{
    public class RoomAvailability
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int RoomId { get; set; }
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? MinNights { get; set; }
        public int? ReservationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Room Room { get; set; } = null!;
        public Company Company { get; set; } = null!;
        public Reservation? Reservation { get; set; }
    }
}