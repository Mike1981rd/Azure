using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Location;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LocationDto>> GetLocationsByCompanyIdAsync(int companyId)
        {
            var locations = await _context.Locations
                .Where(l => l.CompanyId == companyId)
                .OrderByDescending(l => l.IsDefault)
                .ThenBy(l => l.Name)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    CompanyId = l.CompanyId,
                    Name = l.Name,
                    IsDefault = l.IsDefault,
                    FulfillOnlineOrders = l.FulfillOnlineOrders,
                    Address = l.Address,
                    Apartment = l.Apartment,
                    Phone = l.Phone,
                    City = l.City,
                    State = l.State,
                    Country = l.Country,
                    PinCode = l.PinCode,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .ToListAsync();

            return locations;
        }

        public async Task<LocationDto> GetLocationByIdAsync(int id, int companyId)
        {
            var location = await _context.Locations
                .Where(l => l.Id == id && l.CompanyId == companyId)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    CompanyId = l.CompanyId,
                    Name = l.Name,
                    IsDefault = l.IsDefault,
                    FulfillOnlineOrders = l.FulfillOnlineOrders,
                    Address = l.Address,
                    Apartment = l.Apartment,
                    Phone = l.Phone,
                    City = l.City,
                    State = l.State,
                    Country = l.Country,
                    PinCode = l.PinCode,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return location;
        }

        public async Task<LocationDto> CreateLocationAsync(int companyId, CreateLocationDto dto)
        {
            // Check if this is the first location for the company
            var hasLocations = await _context.Locations.AnyAsync(l => l.CompanyId == companyId);

            var location = new Location
            {
                CompanyId = companyId,
                Name = dto.Name,
                IsDefault = !hasLocations, // First location is default
                FulfillOnlineOrders = dto.FulfillOnlineOrders,
                Address = dto.Address,
                Apartment = dto.Apartment,
                Phone = dto.Phone,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PinCode = dto.PinCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return new LocationDto
            {
                Id = location.Id,
                CompanyId = location.CompanyId,
                Name = location.Name,
                IsDefault = location.IsDefault,
                FulfillOnlineOrders = location.FulfillOnlineOrders,
                Address = location.Address,
                Apartment = location.Apartment,
                Phone = location.Phone,
                City = location.City,
                State = location.State,
                Country = location.Country,
                PinCode = location.PinCode,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };
        }

        public async Task<LocationDto> UpdateLocationAsync(int id, int companyId, UpdateLocationDto dto)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id && l.CompanyId == companyId);

            if (location == null)
                return null;

            // If this is the default location and we're disabling fulfillment, prevent it
            if (location.IsDefault && !dto.FulfillOnlineOrders)
            {
                throw new InvalidOperationException("Cannot disable fulfillment for the default location. Set another location as default first.");
            }

            location.Name = dto.Name;
            location.FulfillOnlineOrders = dto.FulfillOnlineOrders;
            location.Address = dto.Address;
            location.Apartment = dto.Apartment;
            location.Phone = dto.Phone;
            location.City = dto.City;
            location.State = dto.State;
            location.Country = dto.Country;
            location.PinCode = dto.PinCode;
            location.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new LocationDto
            {
                Id = location.Id,
                CompanyId = location.CompanyId,
                Name = location.Name,
                IsDefault = location.IsDefault,
                FulfillOnlineOrders = location.FulfillOnlineOrders,
                Address = location.Address,
                Apartment = location.Apartment,
                Phone = location.Phone,
                City = location.City,
                State = location.State,
                Country = location.Country,
                PinCode = location.PinCode,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };
        }

        public async Task<bool> DeleteLocationAsync(int id, int companyId)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id && l.CompanyId == companyId);

            if (location == null)
                return false;

            // Prevent deleting the default location
            if (location.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete the default location. Set another location as default first.");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultLocationAsync(int locationId, int companyId)
        {
            // Get all locations for the company
            var locations = await _context.Locations
                .Where(l => l.CompanyId == companyId)
                .ToListAsync();

            if (locations.Count == 0)
                return false;

            var newDefault = locations.FirstOrDefault(l => l.Id == locationId);
            if (newDefault == null)
                return false;

            // Update all locations
            foreach (var location in locations)
            {
                location.IsDefault = location.Id == locationId;
                if (location.IsDefault)
                {
                    location.FulfillOnlineOrders = true; // Default location must fulfill orders
                }
                location.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LocationDto> GetDefaultLocationAsync(int companyId)
        {
            var location = await _context.Locations
                .Where(l => l.CompanyId == companyId && l.IsDefault)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    CompanyId = l.CompanyId,
                    Name = l.Name,
                    IsDefault = l.IsDefault,
                    FulfillOnlineOrders = l.FulfillOnlineOrders,
                    Address = l.Address,
                    Apartment = l.Apartment,
                    Phone = l.Phone,
                    City = l.City,
                    State = l.State,
                    Country = l.Country,
                    PinCode = l.PinCode,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return location;
        }
    }
}