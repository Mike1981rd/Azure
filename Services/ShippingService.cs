using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Shipping;
using WebsiteBuilderAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsiteBuilderAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShippingService> _logger;

        public ShippingService(ApplicationDbContext context, ILogger<ShippingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Shipping Zones

        public async Task<List<ShippingZoneDto>> GetAllZonesAsync()
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null)
            {
                return new List<ShippingZoneDto>();
            }

            var zones = await _context.ShippingZones
                .Include(z => z.Rates)
                .Where(z => z.CompanyId == company.Id)
                .OrderBy(z => z.DisplayOrder)
                .ThenBy(z => z.Id)
                .ToListAsync();

            return zones.Select(MapToZoneDto).ToList();
        }

        public async Task<ShippingZoneDto?> GetZoneByIdAsync(int id)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return null;

            var zone = await _context.ShippingZones
                .Include(z => z.Rates)
                .FirstOrDefaultAsync(z => z.Id == id && z.CompanyId == company.Id);

            return zone != null ? MapToZoneDto(zone) : null;
        }

        public async Task<ShippingZoneDto> CreateZoneAsync(CreateShippingZoneDto dto)
        {
            _logger.LogInformation("Creating shipping zone with name: {Name}, type: {Type}", dto.Name, dto.ZoneType);
            
            var company = await GetCurrentCompanyAsync();
            if (company == null)
            {
                _logger.LogError("No company found in the system when creating shipping zone");
                throw new InvalidOperationException("No company found in the system");
            }
            
            _logger.LogInformation("Found company with ID: {CompanyId}, Name: {Name}", company.Id, company.Name);

            try
            {
                var zone = new ShippingZone
                {
                    CompanyId = company.Id,
                    Name = dto.Name,
                    ZoneType = dto.ZoneType,
                    Countries = dto.Countries,
                    IsActive = dto.IsActive,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Created ShippingZone object with CompanyId: {CompanyId}", zone.CompanyId);

                // Add default rates for new zone
                zone.Rates = new List<ShippingRate>
            {
                new ShippingRate 
                { 
                    RateType = "weight", 
                    Condition = "",
                    Price = 0,
                    IsActive = true, 
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ShippingRate 
                { 
                    RateType = "vat", 
                    Condition = "",
                    Price = 0,
                    IsActive = true, 
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ShippingRate 
                { 
                    RateType = "duty", 
                    Condition = "",
                    Price = 0,
                    IsActive = true, 
                    DisplayOrder = 3,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

                _logger.LogInformation("Default rates added to zone, about to add to context");
                _context.ShippingZones.Add(zone);
                _logger.LogInformation("Zone added to context, about to save changes");
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created shipping zone {ZoneName} with ID {ZoneId} for company {CompanyId}", zone.Name, zone.Id, company.Id);
                return MapToZoneDto(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateZoneAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ShippingZoneDto?> UpdateZoneAsync(int id, UpdateShippingZoneDto dto)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return null;

            var zone = await _context.ShippingZones
                .Include(z => z.Rates)
                .FirstOrDefaultAsync(z => z.Id == id && z.CompanyId == company.Id);

            if (zone == null) return null;

            // Update only provided fields
            if (dto.Name != null) zone.Name = dto.Name;
            if (dto.ZoneType != null) zone.ZoneType = dto.ZoneType;
            if (dto.Countries != null) zone.Countries = dto.Countries;
            if (dto.IsActive.HasValue) zone.IsActive = dto.IsActive.Value;
            if (dto.DisplayOrder.HasValue) zone.DisplayOrder = dto.DisplayOrder.Value;

            zone.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated shipping zone {ZoneId}", id);

            return MapToZoneDto(zone);
        }

        public async Task<bool> DeleteZoneAsync(int id)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return false;

            var zone = await _context.ShippingZones
                .FirstOrDefaultAsync(z => z.Id == id && z.CompanyId == company.Id);

            if (zone == null) return false;

            _context.ShippingZones.Remove(zone);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted shipping zone {ZoneId}", id);
            return true;
        }

        #endregion

        #region Shipping Rates

        public async Task<ShippingRateDto?> AddRateToZoneAsync(int zoneId, CreateShippingRateDto dto)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return null;

            var zone = await _context.ShippingZones
                .Include(z => z.Rates)
                .FirstOrDefaultAsync(z => z.Id == zoneId && z.CompanyId == company.Id);

            if (zone == null) return null;

            var rate = new ShippingRate
            {
                ShippingZoneId = zoneId,
                RateType = dto.RateType,
                Condition = dto.Condition,
                Price = dto.Price,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            zone.Rates.Add(rate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added rate to zone {ZoneId}", zoneId);
            return MapToRateDto(rate);
        }

        public async Task<ShippingRateDto?> UpdateRateAsync(int zoneId, int rateId, UpdateShippingRateDto dto)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return null;

            var rate = await _context.ShippingRates
                .Include(r => r.ShippingZone)
                .FirstOrDefaultAsync(r => r.Id == rateId && r.ShippingZoneId == zoneId && r.ShippingZone.CompanyId == company.Id);

            if (rate == null) return null;

            // Update only provided fields
            if (dto.RateType != null) rate.RateType = dto.RateType;
            if (dto.Condition != null) rate.Condition = dto.Condition;
            if (dto.Price.HasValue) rate.Price = dto.Price.Value;
            if (dto.IsActive.HasValue) rate.IsActive = dto.IsActive.Value;
            if (dto.DisplayOrder.HasValue) rate.DisplayOrder = dto.DisplayOrder.Value;

            rate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated rate {RateId} in zone {ZoneId}", rateId, zoneId);

            return MapToRateDto(rate);
        }

        public async Task<bool> DeleteRateAsync(int zoneId, int rateId)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return false;

            var rate = await _context.ShippingRates
                .Include(r => r.ShippingZone)
                .FirstOrDefaultAsync(r => r.Id == rateId && r.ShippingZoneId == zoneId && r.ShippingZone.CompanyId == company.Id);

            if (rate == null) return false;

            _context.ShippingRates.Remove(rate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted rate {RateId} from zone {ZoneId}", rateId, zoneId);
            return true;
        }

        #endregion

        #region Bulk Operations

        public async Task<List<ShippingZoneDto>> BulkUpdateZonesAsync(BulkUpdateShippingDto dto)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            var updatedZones = new List<ShippingZone>();

            foreach (var zoneUpdate in dto.Zones)
            {
                var zone = await _context.ShippingZones
                    .Include(z => z.Rates)
                    .FirstOrDefaultAsync(z => z.Id == zoneUpdate.Id && z.CompanyId == company.Id);

                if (zone != null)
                {
                    zone.Name = zoneUpdate.Name;
                    zone.ZoneType = zoneUpdate.ZoneType;
                    zone.Countries = zoneUpdate.Countries;
                    zone.IsActive = zoneUpdate.IsActive;
                    zone.DisplayOrder = zoneUpdate.DisplayOrder;
                    zone.UpdatedAt = DateTime.UtcNow;

                    // Update existing rates and add new ones
                    foreach (var rateUpdate in zoneUpdate.Rates)
                    {
                        if (rateUpdate.Id == 0)
                        {
                            // Create new rate
                            var newRate = new ShippingRate
                            {
                                ShippingZoneId = zone.Id,
                                RateType = rateUpdate.RateType,
                                Condition = rateUpdate.Condition,
                                Price = rateUpdate.Price,
                                IsActive = rateUpdate.IsActive,
                                DisplayOrder = rateUpdate.DisplayOrder,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            zone.Rates.Add(newRate);
                        }
                        else
                        {
                            var rate = zone.Rates.FirstOrDefault(r => r.Id == rateUpdate.Id);
                            if (rate != null)
                            {
                                rate.RateType = rateUpdate.RateType;
                                rate.Condition = rateUpdate.Condition;
                                rate.Price = rateUpdate.Price;
                                rate.IsActive = rateUpdate.IsActive;
                                rate.DisplayOrder = rateUpdate.DisplayOrder;
                                rate.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                    }

                    updatedZones.Add(zone);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk updated {Count} shipping zones", updatedZones.Count);

            return updatedZones.Select(MapToZoneDto).ToList();
        }

        public async Task<ShippingZoneDto?> DuplicateZoneAsync(int id)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null) return null;

            var originalZone = await _context.ShippingZones
                .Include(z => z.Rates)
                .FirstOrDefaultAsync(z => z.Id == id && z.CompanyId == company.Id);

            if (originalZone == null) return null;

            var newZone = new ShippingZone
            {
                CompanyId = company.Id,
                Name = $"{originalZone.Name} (Copy)",
                ZoneType = originalZone.ZoneType,
                Countries = new List<string>(originalZone.Countries),
                IsActive = originalZone.IsActive,
                DisplayOrder = originalZone.DisplayOrder + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Rates = originalZone.Rates.Select(r => new ShippingRate
                {
                    RateType = r.RateType,
                    Condition = r.Condition,
                    Price = r.Price,
                    IsActive = r.IsActive,
                    DisplayOrder = r.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };

            _context.ShippingZones.Add(newZone);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Duplicated zone {OriginalId} to new zone {NewId}", id, newZone.Id);
            return MapToZoneDto(newZone);
        }

        public async Task ReorderZonesAsync(List<int> zoneIds)
        {
            var company = await GetCurrentCompanyAsync();
            if (company == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            var zones = await _context.ShippingZones
                .Where(z => zoneIds.Contains(z.Id) && z.CompanyId == company.Id)
                .ToListAsync();

            for (int i = 0; i < zoneIds.Count; i++)
            {
                var zone = zones.FirstOrDefault(z => z.Id == zoneIds[i]);
                if (zone != null)
                {
                    zone.DisplayOrder = i;
                    zone.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Reordered {Count} shipping zones", zones.Count);
        }

        #endregion

        #region Helper Methods

        public async Task<Company?> GetCurrentCompanyAsync()
        {
            return await _context.Companies.FirstOrDefaultAsync();
        }

        private ShippingZoneDto MapToZoneDto(ShippingZone zone)
        {
            return new ShippingZoneDto
            {
                Id = zone.Id,
                CompanyId = zone.CompanyId,
                Name = zone.Name,
                ZoneType = zone.ZoneType,
                Countries = zone.Countries,
                IsActive = zone.IsActive,
                DisplayOrder = zone.DisplayOrder,
                Rates = zone.Rates.Select(MapToRateDto).ToList(),
                CreatedAt = zone.CreatedAt,
                UpdatedAt = zone.UpdatedAt
            };
        }

        private ShippingRateDto MapToRateDto(ShippingRate rate)
        {
            return new ShippingRateDto
            {
                Id = rate.Id,
                ShippingZoneId = rate.ShippingZoneId,
                RateType = rate.RateType,
                Condition = rate.Condition,
                Price = rate.Price,
                IsActive = rate.IsActive,
                DisplayOrder = rate.DisplayOrder,
                CreatedAt = rate.CreatedAt,
                UpdatedAt = rate.UpdatedAt
            };
        }

        #endregion
    }
}