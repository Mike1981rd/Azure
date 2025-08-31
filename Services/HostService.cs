using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Host;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class HostService
    {
        private readonly ApplicationDbContext _context;
        
        public HostService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<HostListDto>> GetAllHostsAsync(int companyId)
        {
            return await _context.Hosts
                .Where(h => h.CompanyId == companyId)
                .Select(h => new HostListDto
                {
                    Id = h.Id,
                    FirstName = h.FirstName,
                    LastName = h.LastName,
                    FullName = h.FullName,
                    Email = h.Email,
                    ProfilePicture = h.ProfilePicture,
                    OverallRating = h.OverallRating,
                    TotalReviews = h.TotalReviews,
                    ResponseTimeMinutes = h.ResponseTimeMinutes,
                    IsSuperhost = h.IsSuperhost,
                    IsActive = h.IsActive,
                    JoinedDate = h.JoinedDate,
                    RoomCount = h.Rooms.Count(),
                    IsVerified = h.IsVerified
                })
                .OrderByDescending(h => h.JoinedDate)
                .ToListAsync();
        }
        
        private List<string> DeserializeJsonList(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return new List<string>();
                
            try
            {
                // Try to deserialize as a JSON array
                var result = JsonSerializer.Deserialize<List<string>>(jsonString);
                return result ?? new List<string>();
            }
            catch (JsonException)
            {
                // If it fails, check if it's a plain string and wrap it in an array
                try
                {
                    // If it's not valid JSON, treat it as a single string value
                    if (!jsonString.StartsWith("[") && !jsonString.StartsWith("{"))
                    {
                        return new List<string> { jsonString };
                    }
                    
                    // Otherwise return empty list
                    return new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }
        
        public async Task<HostDetailDto?> GetHostByIdAsync(int id, int companyId)
        {
            var host = await _context.Hosts
                .Include(h => h.Rooms)
                .Where(h => h.Id == id && h.CompanyId == companyId)
                .FirstOrDefaultAsync();
                
            if (host == null)
                return null;
                
            return new HostDetailDto
            {
                Id = host.Id,
                UserId = host.UserId,
                FirstName = host.FirstName,
                LastName = host.LastName,
                FullName = host.FullName,
                Email = host.Email,
                PhoneNumber = host.PhoneNumber,
                ProfilePicture = host.ProfilePicture,
                Bio = host.Bio,
                JoinedDate = host.JoinedDate,
                DateOfBirth = host.DateOfBirth,
                YearStartedHosting = host.YearStartedHosting,
                AboutMe = host.AboutMe,
                Location = host.Location,
                Work = host.Work,
                Attributes = DeserializeJsonList(host.Attributes),
                Hobbies = DeserializeJsonList(host.Hobbies),
                IsVerified = host.IsVerified,
                IsPhoneVerified = host.IsPhoneVerified,
                IsEmailVerified = host.IsEmailVerified,
                IsIdentityVerified = host.IsIdentityVerified,
                Languages = DeserializeJsonList(host.Languages),
                OverallRating = host.OverallRating,
                TotalReviews = host.TotalReviews,
                ResponseTimeMinutes = host.ResponseTimeMinutes,
                AcceptanceRate = host.AcceptanceRate,
                IsSuperhost = host.IsSuperhost,
                IsActive = host.IsActive,
                LastActiveDate = host.LastActiveDate,
                RoomCount = host.Rooms.Count(),
                ActiveRoomCount = host.Rooms.Count(r => r.IsActive),
                CreatedAt = host.CreatedAt,
                UpdatedAt = host.UpdatedAt
            };
        }
        
        public async Task<WebsiteBuilderAPI.Models.Host> CreateHostAsync(CreateHostDto dto, string userId)
        {
            var host = new WebsiteBuilderAPI.Models.Host
            {
                UserId = userId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                // FullName is computed property, don't set it
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                ProfilePicture = dto.ProfilePicture,
                Bio = dto.Bio,
                DateOfBirth = dto.DateOfBirth,
                Languages = JsonSerializer.Serialize(dto.Languages ?? new List<string>()),
                IsPhoneVerified = dto.IsPhoneVerified,
                IsEmailVerified = dto.IsEmailVerified,
                IsIdentityVerified = dto.IsIdentityVerified,
                ResponseTimeMinutes = dto.ResponseTimeMinutes,
                IsSuperhost = dto.IsSuperhost,
                IsActive = dto.IsActive,
                CompanyId = dto.CompanyId,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            host.IsVerified = host.IsPhoneVerified && host.IsEmailVerified && host.IsIdentityVerified;
            
            _context.Hosts.Add(host);
            await _context.SaveChangesAsync();
            
            return host;
        }
        
        public async Task<bool> UpdateHostAsync(int id, UpdateHostDto dto, int companyId)
        {
            var host = await _context.Hosts
                .Where(h => h.Id == id && h.CompanyId == companyId)
                .FirstOrDefaultAsync();
                
            if (host == null)
                return false;
                
            if (!string.IsNullOrEmpty(dto.FirstName))
                host.FirstName = dto.FirstName;
                
            if (!string.IsNullOrEmpty(dto.LastName))
                host.LastName = dto.LastName;
                
            // FullName is computed, no need to set it
                
            if (!string.IsNullOrEmpty(dto.Email))
                host.Email = dto.Email;
                
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                host.PhoneNumber = dto.PhoneNumber;
                
            if (!string.IsNullOrEmpty(dto.ProfilePicture))
                host.ProfilePicture = dto.ProfilePicture;
                
            if (!string.IsNullOrEmpty(dto.Bio))
                host.Bio = dto.Bio;
                
            if (dto.DateOfBirth.HasValue)
                host.DateOfBirth = dto.DateOfBirth.Value;
                
            if (dto.YearStartedHosting.HasValue)
                host.YearStartedHosting = dto.YearStartedHosting.Value;
                
            if (!string.IsNullOrEmpty(dto.AboutMe))
                host.AboutMe = dto.AboutMe;
                
            if (!string.IsNullOrEmpty(dto.Location))
                host.Location = dto.Location;
                
            if (!string.IsNullOrEmpty(dto.Work))
                host.Work = dto.Work;
                
            if (dto.Attributes != null)
                host.Attributes = JsonSerializer.Serialize(dto.Attributes);
                
            if (dto.Hobbies != null)
                host.Hobbies = JsonSerializer.Serialize(dto.Hobbies);
                
            if (dto.Languages != null)
                host.Languages = JsonSerializer.Serialize(dto.Languages);
                
            if (dto.IsPhoneVerified.HasValue)
                host.IsPhoneVerified = dto.IsPhoneVerified.Value;
                
            if (dto.IsEmailVerified.HasValue)
                host.IsEmailVerified = dto.IsEmailVerified.Value;
                
            if (dto.IsIdentityVerified.HasValue)
                host.IsIdentityVerified = dto.IsIdentityVerified.Value;
                
            if (dto.ResponseTimeMinutes.HasValue)
                host.ResponseTimeMinutes = dto.ResponseTimeMinutes.Value;
                
            if (dto.IsSuperhost.HasValue)
                host.IsSuperhost = dto.IsSuperhost.Value;
                
            if (dto.IsActive.HasValue)
                host.IsActive = dto.IsActive.Value;
                
            host.IsVerified = host.IsPhoneVerified && host.IsEmailVerified && host.IsIdentityVerified;
            host.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteHostAsync(int id, int companyId)
        {
            var host = await _context.Hosts
                .Where(h => h.Id == id && h.CompanyId == companyId)
                .FirstOrDefaultAsync();
                
            if (host == null)
                return false;
                
            _context.Hosts.Remove(host);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> UpdateHostMetricsAsync(int hostId)
        {
            var host = await _context.Hosts
                .Include(h => h.HostReviews)
                .Where(h => h.Id == hostId)
                .FirstOrDefaultAsync();
                
            if (host == null)
                return false;
                
            if (host.HostReviews.Any())
            {
                host.OverallRating = (decimal)host.HostReviews.Average(r => r.Rating);
                host.TotalReviews = host.HostReviews.Count;
                
                // Update superhost status (e.g., rating > 4.8 and > 10 reviews)
                host.IsSuperhost = host.OverallRating >= 4.8m && host.TotalReviews >= 10;
            }
            
            host.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}