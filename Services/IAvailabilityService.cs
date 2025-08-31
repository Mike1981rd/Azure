using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs.Availability;

namespace WebsiteBuilderAPI.Services
{
    public interface IAvailabilityService
    {
        Task<AvailabilityGridDto> GetAvailabilityGridAsync(int companyId, DateTime startDate, DateTime endDate, int? roomId = null);
        Task<CheckAvailabilityResponseDto> CheckRoomAvailabilityAsync(int companyId, CheckAvailabilityRequestDto request);
        Task<List<RoomAvailabilityDto>> GetRoomAvailabilityAsync(int companyId, int roomId, DateTime startDate, DateTime endDate);
        Task<bool> UpdateRoomAvailabilityAsync(int companyId, int roomId, DateTime date, bool isAvailable, decimal? customPrice = null, int? minNights = null);
        Task<bool> BulkUpdateAvailabilityAsync(int companyId, BulkAvailabilityUpdateDto request);
        
        Task<BlockPeriodDto> CreateBlockPeriodAsync(int companyId, int userId, CreateBlockPeriodDto request);
        Task<List<BlockPeriodDto>> GetBlockPeriodsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> UpdateBlockPeriodAsync(int companyId, int blockPeriodId, CreateBlockPeriodDto request);
        Task<bool> DeleteBlockPeriodAsync(int companyId, int blockPeriodId);
        
        Task<AvailabilityRuleDto> CreateAvailabilityRuleAsync(int companyId, UpdateAvailabilityRuleDto request);
        Task<List<AvailabilityRuleDto>> GetAvailabilityRulesAsync(int companyId, int? roomId = null);
        Task<bool> UpdateAvailabilityRuleAsync(int companyId, int ruleId, UpdateAvailabilityRuleDto request);
        Task<bool> DeleteAvailabilityRuleAsync(int companyId, int ruleId);
        
        Task<OccupancyStatsDto> GetOccupancyStatsAsync(int companyId, DateTime startDate, DateTime endDate);
        Task SyncAvailabilityWithReservationsAsync(int companyId);
        Task<bool> ValidateAvailabilityRulesAsync(int companyId, int roomId, DateTime checkIn, DateTime checkOut);
    }
}