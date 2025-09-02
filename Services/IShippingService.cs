using WebsiteBuilderAPI.DTOs.Shipping;
using WebsiteBuilderAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsiteBuilderAPI.Services
{
    public interface IShippingService
    {
        // Helper Methods
        Task<Company?> GetCurrentCompanyAsync();
        
        // Shipping Zones
        Task<List<ShippingZoneDto>> GetAllZonesAsync();
        Task<ShippingZoneDto?> GetZoneByIdAsync(int id);
        Task<ShippingZoneDto> CreateZoneAsync(CreateShippingZoneDto dto);
        Task<ShippingZoneDto?> UpdateZoneAsync(int id, UpdateShippingZoneDto dto);
        Task<bool> DeleteZoneAsync(int id);

        // Shipping Rates
        Task<ShippingRateDto?> AddRateToZoneAsync(int zoneId, CreateShippingRateDto dto);
        Task<ShippingRateDto?> UpdateRateAsync(int zoneId, int rateId, UpdateShippingRateDto dto);
        Task<bool> DeleteRateAsync(int zoneId, int rateId);

        // Bulk Operations
        Task<List<ShippingZoneDto>> BulkUpdateZonesAsync(BulkUpdateShippingDto dto);
        Task<ShippingZoneDto?> DuplicateZoneAsync(int id);
        Task ReorderZonesAsync(List<int> zoneIds);
    }
}