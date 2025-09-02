using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models.Components;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service implementation for managing structural components
    /// </summary>
    public class StructuralComponentsService : IStructuralComponentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StructuralComponentsService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IWebsiteBuilderCacheService _cacheService;
        private readonly IEditorHistoryService _historyService;

        // Cache keys
        private const string CACHE_KEY_PREFIX = "components:";
        private const string CACHE_KEY_PUBLISHED = "components:published:";
        
        // Cache duration
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        // Default configurations (JSON)
        private const string DEFAULT_HEADER = @"{
            ""colorScheme"":""1"",
            ""width"":""page"",
            ""layout"":""drawer"",
            ""showSeparator"":false,
            ""sticky"":{
                ""enabled"":false,
                ""alwaysShow"":false,
                ""mobileEnabled"":false,
                ""mobileAlwaysShow"":false,
                ""initialOpacity"":100
            },
            ""menuOpenOn"":""hover"",
            ""menuId"":""main-menu"",
            ""logo"":{
                ""desktopUrl"":"""",
                ""height"":190,
                ""mobileUrl"":"""",
                ""mobileHeight"":120,
                ""altText"":""Company Logo"",
                ""link"":""/""
            },
            ""iconStyle"":""style2-outline"",
            ""cart"":{
                ""style"":""bag"",
                ""showCount"":true,
                ""countPosition"":""top-right"",
                ""countBackground"":""#000000"",
                ""countText"":""#FFFFFF""
            },
            ""stickyCart"":false,
            ""edgeRounding"":""size0"",
            ""showAs1"":""drawer-and-page"",
            ""showAs2"":""drawer-and-page"",
            ""customCss"":""""
        }";
        private const string DEFAULT_ANNOUNCEMENT = @"{""enabled"":true,""announcements"":[{""id"":""1"",""content"":""Free shipping on orders over $50!""}]}";
        private const string DEFAULT_FOOTER = @"{""enabled"":true,""sections"":[]}";
        private const string DEFAULT_CART = @"{""displayType"":""drawer"",""drawerPosition"":""right""}";
        private const string DEFAULT_WHATSAPP_WIDGET = @"{
            ""primaryColor"":""#22c55e"",
            ""position"":""bottom-right"",
            ""welcomeMessage"":""¡Hola! 👋 ¿En qué puedo ayudarte hoy?"",
            ""placeholderText"":""Escribe un mensaje..."",
            ""buttonSize"":""medium"",
            ""borderRadius"":50,
            ""showOnMobile"":true,
            ""showOnDesktop"":true,
            ""delaySeconds"":3,
            ""autoOpenSeconds"":0,
            ""isEnabled"":true,
            ""requirementType"":""none"",
            ""requirementTitle"":""Para continuar, por favor ingresa tu información:"",
            ""requirementButtonText"":""Iniciar Chat""
        }";

        public StructuralComponentsService(
            ApplicationDbContext context,
            ILogger<StructuralComponentsService> logger,
            IMemoryCache cache,
            IWebsiteBuilderCacheService cacheService,
            IEditorHistoryService historyService)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _cacheService = cacheService;
            _historyService = historyService;
        }

        #region Get Operations

        public async Task<StructuralComponentsDto?> GetByCompanyIdAsync(int companyId)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{companyId}";
            
            if (_cache.TryGetValue(cacheKey, out StructuralComponentsDto? cached))
            {
                return cached;
            }

            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings if none exist
                settings = await CreateDefaultSettingsAsync(companyId);
            }

            var dto = MapToDto(settings);
            _cache.Set(cacheKey, dto, _cacheDuration);

            return dto;
        }

        public async Task<StructuralComponentsDto?> GetPublishedByCompanyIdAsync(int companyId)
        {
            var cacheKey = $"{CACHE_KEY_PUBLISHED}{companyId}";
            
            if (_cache.TryGetValue(cacheKey, out StructuralComponentsDto? cached))
            {
                return cached;
            }

            // First try to get published version, if not found, get active draft
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsPublished)
                .FirstOrDefaultAsync();
                
            // If no published version, get the active draft (for compatibility)
            if (settings == null)
            {
                settings = await _context.StructuralComponentsSettings
                    .Where(s => s.CompanyId == companyId && s.IsActive)
                    .OrderByDescending(s => s.UpdatedAt)
                    .FirstOrDefaultAsync();
            }

            if (settings != null)
            {
                var dto = MapToDto(settings);
                _cache.Set(cacheKey, dto, TimeSpan.FromHours(24));
                return dto;
            }

            return null;
        }

        public async Task<string?> GetComponentConfigAsync(int companyId, string componentType)
        {
            var settings = await GetByCompanyIdAsync(companyId);
            if (settings == null) return null;

            return componentType.ToLower() switch
            {
                "header" => settings.HeaderConfig,
                "announcementbar" => settings.AnnouncementBarConfig,
                "footer" => settings.FooterConfig,
                "cartdrawer" => settings.CartDrawerConfig,
                "whatsappwidget" => settings.WhatsAppWidgetConfig,
                _ => null
            };
        }

        public async Task<string?> GetPublishedComponentConfigAsync(int companyId, string componentType)
        {
            var settings = await GetPublishedByCompanyIdAsync(companyId);
            if (settings == null) return null;

            return componentType.ToLower() switch
            {
                "header" => settings.HeaderConfig,
                "announcementbar" => settings.AnnouncementBarConfig,
                "footer" => settings.FooterConfig,
                "cartdrawer" => settings.CartDrawerConfig,
                "whatsappwidget" => settings.WhatsAppWidgetConfig,
                _ => null
            };
        }

        #endregion

        #region Create/Update Operations

        public async Task<StructuralComponentsDto> CreateOrUpdateAsync(int companyId, CreateStructuralComponentsDto dto)
        {
            var existing = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing
                existing.HeaderConfig = dto.HeaderConfig ?? existing.HeaderConfig;
                existing.AnnouncementBarConfig = dto.AnnouncementBarConfig ?? existing.AnnouncementBarConfig;
                existing.FooterConfig = dto.FooterConfig ?? existing.FooterConfig;
                existing.ImageBannerConfig = dto.ImageBannerConfig ?? existing.ImageBannerConfig;
                existing.CartDrawerConfig = dto.CartDrawerConfig ?? existing.CartDrawerConfig;
                existing.WhatsAppWidgetConfig = dto.WhatsAppWidgetConfig ?? existing.WhatsAppWidgetConfig;
                existing.Notes = dto.Notes ?? existing.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.Version++;

                await _context.SaveChangesAsync();
                
                // Save history
                await _historyService.SaveHistoryAsync(new CreateHistoryDto
                {
                    EntityType = "components",
                    EntityId = existing.Id,
                    ChangeType = "update",
                    StateData = JsonSerializer.Serialize(existing),
                    Description = "Updated structural components"
                });

                InvalidateCache(companyId);
                _logger.LogInformation("Updated structural components for company {CompanyId}", companyId);
                
                return MapToDto(existing);
            }
            else
            {
                // Create new
                var settings = new StructuralComponentsSettings
                {
                    CompanyId = companyId,
                    HeaderConfig = dto.HeaderConfig ?? DEFAULT_HEADER,
                    AnnouncementBarConfig = dto.AnnouncementBarConfig ?? DEFAULT_ANNOUNCEMENT,
                    FooterConfig = dto.FooterConfig ?? DEFAULT_FOOTER,
                    ImageBannerConfig = dto.ImageBannerConfig ?? "{}",
                    CartDrawerConfig = dto.CartDrawerConfig ?? DEFAULT_CART,
                    WhatsAppWidgetConfig = dto.WhatsAppWidgetConfig ?? DEFAULT_WHATSAPP_WIDGET,
                    Notes = dto.Notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.StructuralComponentsSettings.Add(settings);
                await _context.SaveChangesAsync();

                // Save history
                await _historyService.SaveHistoryAsync(new CreateHistoryDto
                {
                    EntityType = "components",
                    EntityId = settings.Id,
                    ChangeType = "create",
                    StateData = JsonSerializer.Serialize(settings),
                    Description = "Created structural components"
                });

                InvalidateCache(companyId);
                _logger.LogInformation("Created structural components for company {CompanyId}", companyId);
                
                return MapToDto(settings);
            }
        }

        public async Task<StructuralComponentsDto?> UpdateComponentAsync(int companyId, UpdateComponentDto dto)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            // Validate configuration
            if (!await ValidateComponentConfigAsync(dto.ComponentType, dto.Config))
            {
                throw new ArgumentException($"Invalid configuration for {dto.ComponentType}");
            }

            // Update specific component
            switch (dto.ComponentType.ToLower())
            {
                case "header":
                    settings.HeaderConfig = dto.Config;
                    break;
                case "announcementbar":
                    settings.AnnouncementBarConfig = dto.Config;
                    break;
                case "footer":
                    settings.FooterConfig = dto.Config;
                    break;
                case "imagebanner":
                    settings.ImageBannerConfig = dto.Config;
                    break;
                case "cartdrawer":
                    settings.CartDrawerConfig = dto.Config;
                    break;
                case "whatsappwidget":
                    settings.WhatsAppWidgetConfig = dto.Config;
                    break;
                default:
                    throw new ArgumentException($"Unknown component type: {dto.ComponentType}");
            }

            settings.Notes = dto.Notes ?? settings.Notes;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.Version++;

            await _context.SaveChangesAsync();

            // TODO: Fix history service to include CompanyId
            // Save history - temporarily commented due to FK constraint issue
            // await _historyService.SaveHistoryAsync(new CreateHistoryDto
            // {
            //     EntityType = "components",
            //     EntityId = settings.Id,
            //     ChangeType = "update",
            //     StateData = dto.Config,
            //     Description = $"Updated {dto.ComponentType} component"
            // });

            InvalidateCache(companyId);
            _logger.LogInformation("Updated {ComponentType} for company {CompanyId}", 
                dto.ComponentType, companyId);

            return MapToDto(settings);
        }

        public async Task<StructuralComponentsDto?> UpdateAllComponentsAsync(int companyId, StructuralComponentsDto dto)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            settings.HeaderConfig = dto.HeaderConfig;
            settings.AnnouncementBarConfig = dto.AnnouncementBarConfig;
            settings.FooterConfig = dto.FooterConfig;
            settings.ImageBannerConfig = dto.ImageBannerConfig;
            settings.CartDrawerConfig = dto.CartDrawerConfig;
            settings.Notes = dto.Notes;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.Version++;

            await _context.SaveChangesAsync();

            // TODO: Fix history service to include CompanyId
            // Save history - temporarily commented due to FK constraint issue
            // await _historyService.SaveHistoryAsync(new CreateHistoryDto
            // {
            //     EntityType = "components",
            //     EntityId = settings.Id,
            //     ChangeType = "update",
            //     StateData = JsonSerializer.Serialize(settings),
            //     Description = "Updated all structural components"
            // });

            InvalidateCache(companyId);
            _logger.LogInformation("Updated all components for company {CompanyId}", companyId);

            return MapToDto(settings);
        }

        #endregion

        #region Publishing Operations

        public async Task<StructuralComponentsDto?> PublishAsync(int companyId, PublishComponentsDto dto)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            // Create backup if requested
            if (dto.CreateBackup)
            {
                await _historyService.SaveHistoryAsync(companyId, new CreateHistoryDto
                {
                    EntityType = "components",
                    EntityId = settings.Id,
                    ChangeType = "publish",
                    StateData = JsonSerializer.Serialize(settings),
                    Description = "Pre-publish backup",
                    IsCheckpoint = true
                });
            }

            // Mark current published as unpublished
            var currentPublished = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsPublished)
                .ToListAsync();

            foreach (var published in currentPublished)
            {
                published.IsPublished = false;
            }

            // Publish current settings
            settings.IsPublished = true;
            settings.PublishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear all caches
            InvalidateCache(companyId);
            
            // Warm production cache
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            _logger.LogInformation("Published components for company {CompanyId}", companyId);

            return MapToDto(settings);
        }

        public async Task<StructuralComponentsDto?> UnpublishAsync(int companyId)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsPublished)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            settings.IsPublished = false;
            settings.PublishedAt = null;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            InvalidateCache(companyId);
            _logger.LogInformation("Unpublished components for company {CompanyId}", companyId);

            return MapToDto(settings);
        }

        public async Task<StructuralComponentsDto?> CreateDraftFromPublishedAsync(int companyId)
        {
            var published = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsPublished)
                .FirstOrDefaultAsync();

            if (published == null) return null;

            // Deactivate current draft
            var currentDraft = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive && !s.IsPublished)
                .FirstOrDefaultAsync();

            if (currentDraft != null)
            {
                currentDraft.IsActive = false;
            }

            // Create new draft from published
            var draft = new StructuralComponentsSettings
            {
                CompanyId = companyId,
                HeaderConfig = published.HeaderConfig,
                AnnouncementBarConfig = published.AnnouncementBarConfig,
                FooterConfig = published.FooterConfig,
                ImageBannerConfig = published.ImageBannerConfig,
                CartDrawerConfig = published.CartDrawerConfig,
                Notes = "Draft created from published version",
                IsActive = true,
                IsPublished = false,
                Version = published.Version + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StructuralComponentsSettings.Add(draft);
            await _context.SaveChangesAsync();

            InvalidateCache(companyId);
            _logger.LogInformation("Created draft from published for company {CompanyId}", companyId);

            return MapToDto(draft);
        }

        #endregion

        #region Reset Operations

        public async Task<StructuralComponentsDto?> ResetComponentToDefaultAsync(int companyId, string componentType)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            switch (componentType.ToLower())
            {
                case "header":
                    settings.HeaderConfig = DEFAULT_HEADER;
                    break;
                case "announcementbar":
                    settings.AnnouncementBarConfig = DEFAULT_ANNOUNCEMENT;
                    break;
                case "footer":
                    settings.FooterConfig = DEFAULT_FOOTER;
                    break;
                case "imagebanner":
                    settings.ImageBannerConfig = "{}";
                    break;
                case "cartdrawer":
                    settings.CartDrawerConfig = DEFAULT_CART;
                    break;
                default:
                    throw new ArgumentException($"Unknown component type: {componentType}");
            }

            settings.UpdatedAt = DateTime.UtcNow;
            settings.Version++;

            await _context.SaveChangesAsync();

            // Save history
            await _historyService.SaveHistoryAsync(new CreateHistoryDto
            {
                EntityType = "components",
                EntityId = settings.Id,
                ChangeType = "update",
                StateData = JsonSerializer.Serialize(settings),
                Description = $"Reset {componentType} to default"
            });

            InvalidateCache(companyId);
            _logger.LogInformation("Reset {ComponentType} to default for company {CompanyId}", 
                componentType, companyId);

            return MapToDto(settings);
        }

        public async Task<StructuralComponentsDto?> ResetAllToDefaultAsync(int companyId)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .FirstOrDefaultAsync();

            if (settings == null) return null;

            settings.HeaderConfig = DEFAULT_HEADER;
            settings.AnnouncementBarConfig = DEFAULT_ANNOUNCEMENT;
            settings.FooterConfig = DEFAULT_FOOTER;
            settings.ImageBannerConfig = "{}";
            settings.CartDrawerConfig = DEFAULT_CART;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.Version++;

            await _context.SaveChangesAsync();

            // Save history
            await _historyService.SaveHistoryAsync(new CreateHistoryDto
            {
                EntityType = "components",
                EntityId = settings.Id,
                ChangeType = "update",
                StateData = JsonSerializer.Serialize(settings),
                Description = "Reset all components to default"
            });

            InvalidateCache(companyId);
            _logger.LogInformation("Reset all components to default for company {CompanyId}", companyId);

            return MapToDto(settings);
        }

        #endregion

        #region Preview Operations

        public async Task<string> GeneratePreviewHtmlAsync(int companyId, ComponentPreviewDto dto)
        {
            // This would generate HTML for preview
            // Simplified implementation
            var html = $@"
                <div class='component-preview {dto.ComponentType}'>
                    <!-- Component HTML would be generated here based on config -->
                    <pre>{dto.Config}</pre>
                </div>";

            return await Task.FromResult(html);
        }

        public async Task<string> GenerateComponentCssAsync(int companyId, string componentType)
        {
            // This would generate component-specific CSS
            // Simplified implementation
            var css = $@"
                .component-preview.{componentType} {{
                    /* Component styles would be generated here */
                }}";

            return await Task.FromResult(css);
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateComponentConfigAsync(string componentType, string config)
        {
            try
            {
                // Parse JSON to validate structure
                var jsonDoc = JsonDocument.Parse(config);
                
                // TODO: Add component-specific validation
                
                return await Task.FromResult(true);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public async Task<ValidationResult> ValidateAllComponentsAsync(int companyId)
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new System.Collections.Generic.List<string>();
            var warnings = new System.Collections.Generic.List<string>();

            var settings = await GetByCompanyIdAsync(companyId);
            if (settings == null)
            {
                errors.Add("No component settings found");
                result.IsValid = false;
            }
            else
            {
                // Validate each component
                if (!await ValidateComponentConfigAsync("header", settings.HeaderConfig))
                    errors.Add("Invalid header configuration");
                
                if (!await ValidateComponentConfigAsync("announcementbar", settings.AnnouncementBarConfig))
                    errors.Add("Invalid announcement bar configuration");
                
                if (!await ValidateComponentConfigAsync("footer", settings.FooterConfig))
                    errors.Add("Invalid footer configuration");
                
                if (!await ValidateComponentConfigAsync("cartdrawer", settings.CartDrawerConfig))
                    errors.Add("Invalid cart drawer configuration");
            }

            result.Errors = errors.ToArray();
            result.Warnings = warnings.ToArray();
            result.IsValid = errors.Count == 0;

            return result;
        }

        #endregion

        #region Version Management

        public async Task<int> GetCurrentVersionAsync(int companyId)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .Select(s => s.Version)
                .FirstOrDefaultAsync();

            return settings;
        }

        public async Task<StructuralComponentsDto?> GetVersionAsync(int companyId, int version)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId && s.Version == version)
                .FirstOrDefaultAsync();

            return settings != null ? MapToDto(settings) : null;
        }

        public async Task<StructuralComponentsDto[]> GetVersionHistoryAsync(int companyId, int limit = 10)
        {
            var settings = await _context.StructuralComponentsSettings
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.Version)
                .Take(limit)
                .ToListAsync();

            return settings.Select(MapToDto).ToArray();
        }

        #endregion

        #region Helper Methods

        private async Task<StructuralComponentsSettings> CreateDefaultSettingsAsync(int companyId)
        {
            var settings = new StructuralComponentsSettings
            {
                CompanyId = companyId,
                HeaderConfig = DEFAULT_HEADER,
                AnnouncementBarConfig = DEFAULT_ANNOUNCEMENT,
                FooterConfig = DEFAULT_FOOTER,
                ImageBannerConfig = "{}",
                CartDrawerConfig = DEFAULT_CART,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StructuralComponentsSettings.Add(settings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created default structural components for company {CompanyId}", companyId);

            return settings;
        }

        private StructuralComponentsDto MapToDto(StructuralComponentsSettings settings)
        {
            return new StructuralComponentsDto
            {
                Id = settings.Id,
                CompanyId = settings.CompanyId,
                HeaderConfig = settings.HeaderConfig,
                AnnouncementBarConfig = settings.AnnouncementBarConfig,
                FooterConfig = settings.FooterConfig,
                ImageBannerConfig = settings.ImageBannerConfig,
                CartDrawerConfig = settings.CartDrawerConfig,
                WhatsAppWidgetConfig = settings.WhatsAppWidgetConfig,
                IsActive = settings.IsActive,
                IsPublished = settings.IsPublished,
                PublishedAt = settings.PublishedAt,
                Version = settings.Version,
                Notes = settings.Notes,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt
            };
        }

        private void InvalidateCache(int companyId)
        {
            _cache.Remove($"{CACHE_KEY_PREFIX}{companyId}");
            _cache.Remove($"{CACHE_KEY_PUBLISHED}{companyId}");
        }

        #endregion
    }
}