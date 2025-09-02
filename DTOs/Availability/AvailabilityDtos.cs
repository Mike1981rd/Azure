using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Availability
{
    public class RoomAvailabilityDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBlocked { get; set; }
        public bool HasReservation { get; set; }
        public string? BlockReason { get; set; }
        public decimal Price { get; set; }
        public int? ReservationId { get; set; }
        public string? GuestName { get; set; }
        public string? ReservationStatus { get; set; }
        public bool IsCheckIn { get; set; }
        public bool IsCheckOut { get; set; }
    }

    public class DayAvailabilityDto
    {
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBlocked { get; set; }
        public bool HasReservation { get; set; }
        public string? BlockReason { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? ReservationId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestInitials { get; set; }
        public bool IsCheckIn { get; set; }
        public bool IsCheckOut { get; set; }
        public bool IsToday { get; set; }
        public int? MinNights { get; set; }
    }

    public class AvailabilityGridDto
    {
        public List<RoomDto> Rooms { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();
        public Dictionary<string, DayAvailabilityDto> Availability { get; set; } = new();
        public int TotalRooms { get; set; }
        public int TotalDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? RoomCode { get; set; }
        public string? RoomType { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }
        public int? FloorNumber { get; set; }
    }

    public class CreateBlockPeriodDto
    {
        public List<int>? RoomIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
    }

    public class BlockPeriodDto
    {
        public int Id { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public string? RoomNames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateAvailabilityRuleDto
    {
        public int? RoomId { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public Dictionary<string, object> RuleConfig { get; set; } = new();
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AvailabilityRuleDto
    {
        public int Id { get; set; }
        public int? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public string RuleTypeLabel { get; set; } = string.Empty;
        public Dictionary<string, object> RuleConfig { get; set; } = new();
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OccupancyStatsDto
    {
        public int TotalRooms { get; set; }
        public int TotalNights { get; set; }
        public int OccupiedNights { get; set; }
        public int AvailableToday { get; set; }
        public int BlockedToday { get; set; }
        public decimal OccupancyRate { get; set; }
        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
        public decimal ProjectedRevenue { get; set; }
        public Dictionary<string, decimal> OccupancyByRoomType { get; set; } = new();
        public List<DailyOccupancyDto> DailyStats { get; set; } = new();
    }

    public class DailyOccupancyDto
    {
        public DateTime Date { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int BlockedRooms { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CheckAvailabilityRequestDto
    {
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int? ExcludeReservationId { get; set; }
    }

    public class CheckAvailabilityResponseDto
    {
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
        public List<DateTime> BlockedDates { get; set; } = new();
        public List<DateTime> ReservedDates { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public int MinNightsRequired { get; set; }
        public List<string> AppliedRules { get; set; } = new();
    }

    public class BulkAvailabilityUpdateDto
    {
        public List<int> RoomIds { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? MinNights { get; set; }
    }
}