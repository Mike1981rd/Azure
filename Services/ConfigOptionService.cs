using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.ConfigOptions;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IConfigOptionService
    {
        Task<List<ConfigOptionDto>> GetByTypeAsync(int companyId, string type);
        Task<List<ConfigOptionDto>> GetAllAsync(int companyId);
        Task<ConfigOptionDto?> GetByIdAsync(int companyId, int id);
        Task<ConfigOptionDto> CreateAsync(int companyId, CreateConfigOptionDto dto);
        Task<ConfigOptionDto?> UpdateAsync(int companyId, int id, UpdateConfigOptionDto dto);
        Task<bool> DeleteAsync(int companyId, int id);
        Task IncrementUsageAsync(int companyId, string type, string value);
        Task InitializeDefaultOptionsAsync(int companyId);
    }

    public class ConfigOptionService : IConfigOptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfigOptionService> _logger;

        public ConfigOptionService(ApplicationDbContext context, ILogger<ConfigOptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ConfigOptionDto>> GetByTypeAsync(int companyId, string type)
        {
            var options = await _context.ConfigOptions
                .Where(o => o.CompanyId == companyId && o.Type == type && o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ThenByDescending(o => o.UsageCount)
                .ThenBy(o => o.LabelEs)
                .Select(o => MapToDto(o))
                .ToListAsync();

            return options;
        }

        public async Task<List<ConfigOptionDto>> GetAllAsync(int companyId)
        {
            var options = await _context.ConfigOptions
                .Where(o => o.CompanyId == companyId)
                .OrderBy(o => o.Type)
                .ThenBy(o => o.SortOrder)
                .ThenBy(o => o.LabelEs)
                .Select(o => MapToDto(o))
                .ToListAsync();

            return options;
        }

        public async Task<ConfigOptionDto?> GetByIdAsync(int companyId, int id)
        {
            var option = await _context.ConfigOptions
                .Where(o => o.CompanyId == companyId && o.Id == id)
                .Select(o => MapToDto(o))
                .FirstOrDefaultAsync();

            return option;
        }

        public async Task<ConfigOptionDto> CreateAsync(int companyId, CreateConfigOptionDto dto)
        {
            // Verificar si ya existe una opción con el mismo valor y tipo
            var exists = await _context.ConfigOptions
                .AnyAsync(o => o.CompanyId == companyId && 
                              o.Type == dto.Type && 
                              o.Value == dto.Value);

            if (exists)
            {
                throw new InvalidOperationException($"Ya existe una opción '{dto.Value}' para el tipo '{dto.Type}'");
            }

            var option = new ConfigOption
            {
                CompanyId = companyId,
                Type = dto.Type,
                Value = dto.Value,
                LabelEs = dto.LabelEs,
                LabelEn = dto.LabelEn,
                Icon = dto.Icon,
                IconType = dto.IconType ?? "heroicon",
                Category = dto.Category,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                IsCustom = true,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConfigOptions.Add(option);
            await _context.SaveChangesAsync();

            _logger.LogInformation("ConfigOption creada: {Type}/{Value} para Company {CompanyId}", 
                dto.Type, dto.Value, companyId);

            return MapToDto(option);
        }

        public async Task<ConfigOptionDto?> UpdateAsync(int companyId, int id, UpdateConfigOptionDto dto)
        {
            var option = await _context.ConfigOptions
                .FirstOrDefaultAsync(o => o.CompanyId == companyId && o.Id == id);

            if (option == null)
                return null;

            if (dto.LabelEs != null)
                option.LabelEs = dto.LabelEs;
            
            if (dto.LabelEn != null)
                option.LabelEn = dto.LabelEn;
            
            if (dto.Icon != null)
                option.Icon = dto.Icon;
            
            if (dto.IconType != null)
                option.IconType = dto.IconType;
            
            if (dto.Category != null)
                option.Category = dto.Category;
            
            if (dto.SortOrder.HasValue)
                option.SortOrder = dto.SortOrder.Value;
            
            if (dto.IsActive.HasValue)
                option.IsActive = dto.IsActive.Value;

            option.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("ConfigOption actualizada: {Id} para Company {CompanyId}", 
                id, companyId);

            return MapToDto(option);
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            var option = await _context.ConfigOptions
                .FirstOrDefaultAsync(o => o.CompanyId == companyId && o.Id == id);

            if (option == null)
                return false;

            // No permitir eliminar opciones por defecto
            if (option.IsDefault)
            {
                throw new InvalidOperationException("No se pueden eliminar opciones por defecto. Puede desactivarlas en su lugar.");
            }

            _context.ConfigOptions.Remove(option);
            await _context.SaveChangesAsync();

            _logger.LogInformation("ConfigOption eliminada: {Id} para Company {CompanyId}", 
                id, companyId);

            return true;
        }

        public async Task IncrementUsageAsync(int companyId, string type, string value)
        {
            var option = await _context.ConfigOptions
                .FirstOrDefaultAsync(o => o.CompanyId == companyId && 
                                         o.Type == type && 
                                         o.Value == value);

            if (option != null)
            {
                option.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task InitializeDefaultOptionsAsync(int companyId)
        {
            // Verificar si ya existen opciones POR DEFECTO para esta empresa
            var hasDefaultOptions = await _context.ConfigOptions
                .AnyAsync(o => o.CompanyId == companyId && o.IsDefault == true);

            if (hasDefaultOptions)
            {
                _logger.LogInformation("Ya existen opciones por defecto para Company {CompanyId}", companyId);
                return;
            }

            _logger.LogInformation("Inicializando opciones por defecto para Company {CompanyId}", companyId);

            var defaultOptions = GetDefaultOptions(companyId);
            
            // Verificar y agregar solo las opciones que no existen
            foreach (var option in defaultOptions)
            {
                var exists = await _context.ConfigOptions
                    .AnyAsync(o => o.CompanyId == companyId && 
                                  o.Type == option.Type && 
                                  o.Value == option.Value);
                
                if (!exists)
                {
                    _context.ConfigOptions.Add(option);
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Se agregaron {Count} opciones por defecto para Company {CompanyId}", 
                defaultOptions.Count, companyId);
        }

        private static List<ConfigOption> GetDefaultOptions(int companyId)
        {
            var options = new List<ConfigOption>();

            // Amenidades por defecto
            var amenities = new[]
            {
                ("wifi", "WiFi", "WiFi", "wifi", "básicas"),
                ("tv", "TV", "TV", "tv", "básicas"),
                ("ac", "Aire Acondicionado", "Air Conditioning", "snowflake", "básicas"),
                ("heating", "Calefacción", "Heating", "fire", "básicas"),
                ("minibar", "Minibar", "Minibar", "cube", "premium"),
                ("safe", "Caja Fuerte", "Safe", "lock-closed", "básicas"),
                ("balcony", "Balcón", "Balcony", "home-modern", "exterior"),
                ("terrace", "Terraza", "Terrace", "sun", "exterior"),
                ("jacuzzi", "Jacuzzi", "Jacuzzi", "sparkles", "premium"),
                ("pool", "Piscina", "Pool", "🏊", "exterior"),
                ("gym", "Gimnasio", "Gym", "🏋️", "premium"),
                ("parking", "Estacionamiento", "Parking", "🚗", "básicas"),
                ("kitchen", "Cocina", "Kitchen", "🍳", "básicas"),
                ("microwave", "Microondas", "Microwave", "square-3-stack-3d", "básicas"),
                ("coffee", "Cafetera", "Coffee Maker", "☕", "básicas"),
                ("hairdryer", "Secador de Pelo", "Hair Dryer", "🌬️", "básicas"),
                ("iron", "Plancha", "Iron", "square-2-stack", "básicas"),
                ("washer", "Lavadora", "Washer", "🧺", "premium"),
                ("dryer", "Secadora", "Dryer", "sun", "premium"),
                ("dishwasher", "Lavavajillas", "Dishwasher", "squares-2x2", "premium")
            };

            int sortOrder = 0;
            foreach (var (value, labelEs, labelEn, icon, category) in amenities)
            {
                var iconType = icon.Length == 1 || icon.StartsWith("�") ? "emoji" : "heroicon";
                options.Add(new ConfigOption
                {
                    CompanyId = companyId,
                    Type = "amenity",
                    Value = value,
                    LabelEs = labelEs,
                    LabelEn = labelEn,
                    Icon = icon,
                    IconType = iconType,
                    Category = category,
                    SortOrder = sortOrder++,
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Tipos de habitación por defecto
            var roomTypes = new[]
            {
                ("standard", "Estándar", "Standard", "home"),
                ("deluxe", "Deluxe", "Deluxe", "star"),
                ("suite", "Suite", "Suite", "sparkles"),
                ("junior_suite", "Junior Suite", "Junior Suite", "star"),
                ("presidential", "Presidencial", "Presidential", "🏆"),
                ("penthouse", "Penthouse", "Penthouse", "building-office-2"),
                ("studio", "Estudio", "Studio", "square-2-stack"),
                ("apartment", "Apartamento", "Apartment", "building-office"),
                ("villa", "Villa", "Villa", "🏡"),
                ("bungalow", "Bungalow", "Bungalow", "🏠")
            };

            sortOrder = 0;
            foreach (var (value, labelEs, labelEn, icon) in roomTypes)
            {
                var iconType = icon.StartsWith("�") ? "emoji" : "heroicon";
                options.Add(new ConfigOption
                {
                    CompanyId = companyId,
                    Type = "room_type",
                    Value = value,
                    LabelEs = labelEs,
                    LabelEn = labelEn,
                    Icon = icon,
                    IconType = iconType,
                    SortOrder = sortOrder++,
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Tipos de vista por defecto
            var viewTypes = new[]
            {
                ("sea", "Vista al Mar", "Sea View", "🌊"),
                ("city", "Vista a la Ciudad", "City View", "🏙️"),
                ("garden", "Vista al Jardín", "Garden View", "🌳"),
                ("pool", "Vista a la Piscina", "Pool View", "🏊"),
                ("mountain", "Vista a la Montaña", "Mountain View", "⛰️"),
                ("interior", "Vista Interior", "Interior View", "home"),
                ("lake", "Vista al Lago", "Lake View", "🏞️"),
                ("river", "Vista al Río", "River View", "🌊"),
                ("forest", "Vista al Bosque", "Forest View", "🌲")
            };

            sortOrder = 0;
            foreach (var (value, labelEs, labelEn, icon) in viewTypes)
            {
                var iconType = icon.StartsWith("�") || icon.Length == 1 ? "emoji" : "heroicon";
                options.Add(new ConfigOption
                {
                    CompanyId = companyId,
                    Type = "view_type",
                    Value = value,
                    LabelEs = labelEs,
                    LabelEn = labelEn,
                    Icon = icon,
                    IconType = iconType,
                    SortOrder = sortOrder++,
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return options;
        }

        private static ConfigOptionDto MapToDto(ConfigOption option)
        {
            return new ConfigOptionDto
            {
                Id = option.Id,
                Type = option.Type,
                Value = option.Value,
                LabelEs = option.LabelEs,
                LabelEn = option.LabelEn,
                Icon = option.Icon,
                IconType = option.IconType,
                Category = option.Category,
                SortOrder = option.SortOrder,
                UsageCount = option.UsageCount,
                IsActive = option.IsActive,
                IsCustom = option.IsCustom,
                IsDefault = option.IsDefault
            };
        }
    }
}