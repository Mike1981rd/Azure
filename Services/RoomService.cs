using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Rooms;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IRoomService
    {
        Task<List<RoomResponseDto>> GetAllAsync(int companyId);
        Task<RoomResponseDto?> GetByIdAsync(int companyId, int id);
        Task<RoomResponseDto?> GetBySlugAsync(int companyId, string slug);
        Task<RoomResponseDto> CreateAsync(int companyId, CreateRoomDto dto);
        Task<RoomResponseDto?> UpdateAsync(int companyId, int id, UpdateRoomDto dto);
        Task<bool> DeleteAsync(int companyId, int id);
        Task<bool> ExistsAsync(int companyId, int id);
    }

    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomResponseDto>> GetAllAsync(int companyId)
        {
            var rooms = await _context.Rooms
                .Include(r => r.Host) // Include Host data
                .Where(r => r.CompanyId == companyId)
                .OrderBy(r => r.RoomCode ?? r.Name)
                .ToListAsync();

            return rooms.Select(MapToDto).ToList();
        }

        public async Task<RoomResponseDto?> GetByIdAsync(int companyId, int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Host) // Include Host data
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            return room != null ? MapToDto(room) : null;
        }

        public async Task<RoomResponseDto?> GetBySlugAsync(int companyId, string slug)
        {
            var room = await _context.Rooms
                .Include(r => r.Host) // Include Host data
                .FirstOrDefaultAsync(r => r.Slug == slug && r.CompanyId == companyId && r.IsActive);

            return room != null ? MapToDto(room) : null;
        }

        public async Task<RoomResponseDto> CreateAsync(int companyId, CreateRoomDto dto)
        {
            var room = new Room
            {
                CompanyId = companyId,
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                ComparePrice = dto.ComparePrice,
                MaxOccupancy = dto.MaxOccupancy,
                RoomCode = dto.RoomCode,
                RoomType = dto.RoomType,
                FloorNumber = dto.FloorNumber,
                ViewType = dto.ViewType,
                SquareMeters = dto.SquareMeters,
                // NUEVOS CAMPOS - Ubicación
                StreetAddress = dto.StreetAddress,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                Neighborhood = dto.Neighborhood,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                // Host
                HostId = dto.HostId,
                // Campos JSONB - Convertir de object a JsonDocument
                SleepingArrangements = dto.SleepingArrangements != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.SleepingArrangements)) : null,
                HouseRules = dto.HouseRules != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.HouseRules)) : null,
                CancellationPolicy = dto.CancellationPolicy != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.CancellationPolicy)) : null,
                CheckInInstructions = dto.CheckInInstructions != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.CheckInInstructions)) : null,
                SafetyFeatures = dto.SafetyFeatures != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.SafetyFeatures)) : null,
                HighlightFeatures = dto.HighlightFeatures != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.HighlightFeatures)) : null,
                AdditionalFees = dto.AdditionalFees != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.AdditionalFees)) : null,
                SafetyAndProperty = dto.SafetyAndProperty != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.SafetyAndProperty)) : null,
                GuestMaximum = dto.GuestMaximum != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.GuestMaximum)) : null,
                RoomDetails = dto.RoomDetails != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.RoomDetails)) : null,
                CommonSpaces = dto.CommonSpaces != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.CommonSpaces)) : null,
                Highlights = dto.Highlights != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.Highlights)) : null, // NUEVO - Campo Highlights agregado
                // Órdenes de visualización
                HouseRulesOrder = dto.HouseRulesOrder,
                CancellationPolicyOrder = dto.CancellationPolicyOrder,
                SafetyAndPropertyOrder = dto.SafetyAndPropertyOrder,
                // Campos SEO
                Slug = !string.IsNullOrEmpty(dto.Slug) ? dto.Slug : GenerateSlug(dto.Name),
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                // Estadísticas (valores iniciales por defecto)
                TotalReviews = 0,
                AverageRating = 0,
                ResponseTime = null,
                Tags = dto.Tags,
                Amenities = dto.Amenities,
                Images = dto.Images,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return MapToDto(room);
        }

        public async Task<RoomResponseDto?> UpdateAsync(int companyId, int id, UpdateRoomDto dto)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (room == null)
                return null;

            // Actualizar solo los campos proporcionados - manejar strings vacíos explícitamente
            if (dto.Name != null && dto.Name != "")
                room.Name = dto.Name;
            
            // Description puede ser vacío (null)
            if (dto.Description != null)
                room.Description = dto.Description == "" ? null : dto.Description;
            
            if (dto.BasePrice.HasValue) 
                room.BasePrice = dto.BasePrice.Value;
            
            // ComparePrice: si se envía 0, se interpreta como eliminar el precio de comparación
            if (dto.ComparePrice.HasValue)
                room.ComparePrice = dto.ComparePrice.Value == 0 ? null : dto.ComparePrice.Value;
            
            if (dto.MaxOccupancy.HasValue) 
                room.MaxOccupancy = dto.MaxOccupancy.Value;
            
            // RoomCode puede ser vacío (null)
            if (dto.RoomCode != null)
                room.RoomCode = dto.RoomCode == "" ? null : dto.RoomCode;
            
            // RoomType puede ser vacío (null)
            if (dto.RoomType != null)
                room.RoomType = dto.RoomType == "" ? null : dto.RoomType;
            
            // FloorNumber: si se envía 0, se interpreta como planta baja, no como eliminar
            if (dto.FloorNumber.HasValue)
                room.FloorNumber = dto.FloorNumber.Value;
            
            // ViewType puede ser vacío (null)
            if (dto.ViewType != null)
                room.ViewType = dto.ViewType == "" ? null : dto.ViewType;
            
            // SquareMeters: si se envía 0, se interpreta como eliminar el valor
            if (dto.SquareMeters.HasValue)
                room.SquareMeters = dto.SquareMeters.Value == 0 ? null : dto.SquareMeters.Value;
            
            // NUEVOS CAMPOS - Ubicación
            if (dto.StreetAddress != null)
                room.StreetAddress = dto.StreetAddress == "" ? null : dto.StreetAddress;
            
            if (dto.City != null)
                room.City = dto.City == "" ? null : dto.City;
            
            if (dto.State != null)
                room.State = dto.State == "" ? null : dto.State;
            
            if (dto.Country != null)
                room.Country = dto.Country == "" ? null : dto.Country;
            
            if (dto.PostalCode != null)
                room.PostalCode = dto.PostalCode == "" ? null : dto.PostalCode;
            
            if (dto.Neighborhood != null)
                room.Neighborhood = dto.Neighborhood == "" ? null : dto.Neighborhood;
            
            if (dto.Latitude.HasValue)
                room.Latitude = dto.Latitude.Value == 0 ? null : dto.Latitude.Value;
            
            if (dto.Longitude.HasValue)
                room.Longitude = dto.Longitude.Value == 0 ? null : dto.Longitude.Value;
            
            // Host
            if (dto.HostId.HasValue)
                room.HostId = dto.HostId.Value == 0 ? null : dto.HostId.Value;
            
            // Campos JSONB - Convertir de object a JsonDocument
            if (dto.SleepingArrangements != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.SleepingArrangements);
                room.SleepingArrangements = JsonDocument.Parse(json);
            }
            
            if (dto.HouseRules != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.HouseRules);
                room.HouseRules = JsonDocument.Parse(json);
            }
            
            if (dto.CancellationPolicy != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.CancellationPolicy);
                room.CancellationPolicy = JsonDocument.Parse(json);
            }
            
            if (dto.CheckInInstructions != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.CheckInInstructions);
                room.CheckInInstructions = JsonDocument.Parse(json);
            }
            
            if (dto.SafetyFeatures != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.SafetyFeatures);
                room.SafetyFeatures = JsonDocument.Parse(json);
            }
            
            if (dto.HighlightFeatures != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.HighlightFeatures);
                room.HighlightFeatures = JsonDocument.Parse(json);
            }
            
            if (dto.AdditionalFees != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalFees);
                room.AdditionalFees = JsonDocument.Parse(json);
            }
            
            if (dto.SafetyAndProperty != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.SafetyAndProperty);
                room.SafetyAndProperty = JsonDocument.Parse(json);
            }
            
            if (dto.GuestMaximum != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.GuestMaximum);
                room.GuestMaximum = JsonDocument.Parse(json);
            }
            
            if (dto.RoomDetails != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.RoomDetails);
                room.RoomDetails = JsonDocument.Parse(json);
            }
            
            if (dto.CommonSpaces != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.CommonSpaces);
                room.CommonSpaces = JsonDocument.Parse(json);
            }
            
            // NUEVO - Campo Highlights agregado
            if (dto.Highlights != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(dto.Highlights);
                room.Highlights = JsonDocument.Parse(json);
            }
            
            // Campos SEO
            if (dto.Slug != null)
            {
                // Si viene vacío y el room no tiene slug, generar uno basado en el nombre
                if (string.IsNullOrEmpty(dto.Slug) && string.IsNullOrEmpty(room.Slug))
                {
                    room.Slug = GenerateSlug(room.Name);
                }
                else if (!string.IsNullOrEmpty(dto.Slug))
                {
                    room.Slug = dto.Slug;
                }
            }
            else if (string.IsNullOrEmpty(room.Slug))
            {
                // Si no se envía slug y el room no tiene uno, generarlo
                room.Slug = GenerateSlug(room.Name);
            }
            
            if (dto.MetaTitle != null)
                room.MetaTitle = dto.MetaTitle == "" ? null : dto.MetaTitle;
            
            if (dto.MetaDescription != null)
                room.MetaDescription = dto.MetaDescription == "" ? null : dto.MetaDescription;
            
            // Tags y Amenities: lista vacía es válida
            if (dto.Tags != null) 
                room.Tags = dto.Tags;
            
            if (dto.Amenities != null) 
                room.Amenities = dto.Amenities;
            
            if (dto.Images != null) 
                room.Images = dto.Images;
            
            if (dto.IsActive.HasValue) 
                room.IsActive = dto.IsActive.Value;

            // Órdenes de visualización
            if (dto.HouseRulesOrder != null)
                room.HouseRulesOrder = dto.HouseRulesOrder;
            if (dto.CancellationPolicyOrder != null)
                room.CancellationPolicyOrder = dto.CancellationPolicyOrder;
            if (dto.SafetyAndPropertyOrder != null)
                room.SafetyAndPropertyOrder = dto.SafetyAndPropertyOrder;

            room.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(room);
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int companyId, int id)
        {
            return await _context.Rooms
                .AnyAsync(r => r.Id == id && r.CompanyId == companyId);
        }

        private static RoomResponseDto MapToDto(Room room)
        {
            return new RoomResponseDto
            {
                Id = room.Id,
                CompanyId = room.CompanyId,
                Name = room.Name,
                Description = room.Description,
                BasePrice = room.BasePrice,
                ComparePrice = room.ComparePrice,
                MaxOccupancy = room.MaxOccupancy,
                RoomCode = room.RoomCode,
                RoomType = room.RoomType,
                FloorNumber = room.FloorNumber,
                ViewType = room.ViewType,
                SquareMeters = room.SquareMeters,
                // NUEVOS CAMPOS - Ubicación
                StreetAddress = room.StreetAddress,
                City = room.City,
                State = room.State,
                Country = room.Country,
                PostalCode = room.PostalCode,
                Neighborhood = room.Neighborhood,
                Latitude = room.Latitude,
                Longitude = room.Longitude,
                // Host
                HostId = room.HostId,
                Host = room.Host != null ? new {
                    id = room.Host.Id,
                    firstName = room.Host.FirstName,
                    lastName = room.Host.LastName,
                    fullName = room.Host.FullName,
                    profilePicture = room.Host.ProfilePicture,
                    bio = room.Host.Bio,
                    joinedDate = room.Host.JoinedDate,
                    isVerified = room.Host.IsVerified,
                    isSuperhost = room.Host.IsSuperhost,
                    overallRating = room.Host.OverallRating,
                    totalReviews = room.Host.TotalReviews,
                    responseTimeMinutes = room.Host.ResponseTimeMinutes,
                    languages = room.Host.Languages
                } : null,
                // Campos JSONB
                SleepingArrangements = room.SleepingArrangements,
                HouseRules = room.HouseRules,
                CancellationPolicy = room.CancellationPolicy,
                CheckInInstructions = room.CheckInInstructions,
                SafetyFeatures = room.SafetyFeatures,
                HighlightFeatures = room.HighlightFeatures,
                AdditionalFees = room.AdditionalFees,
                SafetyAndProperty = room.SafetyAndProperty,
                GuestMaximum = room.GuestMaximum,
                RoomDetails = room.RoomDetails,
                CommonSpaces = room.CommonSpaces,
                Highlights = room.Highlights, // NUEVO - Campo Highlights agregado
                // Órdenes de visualización
                HouseRulesOrder = room.HouseRulesOrder,
                CancellationPolicyOrder = room.CancellationPolicyOrder,
                SafetyAndPropertyOrder = room.SafetyAndPropertyOrder,
                // Campos SEO
                Slug = room.Slug,
                MetaTitle = room.MetaTitle,
                MetaDescription = room.MetaDescription,
                // Estadísticas (no editables desde UI, calculadas por sistema)
                TotalReviews = room.TotalReviews,
                AverageRating = room.AverageRating,
                ResponseTime = room.ResponseTime,
                Tags = room.Tags,
                Amenities = room.Amenities,
                Images = room.Images,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            
            // Convert to lowercase
            var slug = name.ToLowerInvariant();
            
            // Replace accented characters with their non-accented equivalents
            slug = slug.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                      .Replace("ñ", "n").Replace("ü", "u")
                      .Replace("à", "a").Replace("è", "e").Replace("ì", "i").Replace("ò", "o").Replace("ù", "u");
            
            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            
            // Remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // Replace multiple hyphens with single hyphen
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            slug = slug.Trim('-');
            
            return slug;
        }
    }
}