using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models.ThemeConfig;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service implementation for managing global theme configurations
    /// Uses caching for performance and modular updates to avoid large JSON transfers
    /// </summary>
    public class GlobalThemeConfigService : IGlobalThemeConfigService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GlobalThemeConfigService> _logger;
        private const string CACHE_KEY_PREFIX = "ThemeConfig_";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        public GlobalThemeConfigService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<GlobalThemeConfigService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<GlobalThemeConfig> GetByCompanyIdAsync(int companyId)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{companyId}";
            
            if (_cache.TryGetValue(cacheKey, out GlobalThemeConfig cachedConfig))
            {
                _logger.LogDebug("Theme config retrieved from cache for company {CompanyId}", companyId);
                return cachedConfig;
            }

            _logger.LogInformation("Fetching theme config from database for company {CompanyId}", companyId);
            
            var config = await _context.GlobalThemeConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
            {
                _logger.LogInformation("No theme config found for company {CompanyId}, creating default", companyId);
                config = await CreateDefaultConfigAsync(companyId);
                _logger.LogInformation("Default theme config created for company {CompanyId}", companyId);
            }
            else
            {
                _logger.LogInformation("Theme config found for company {CompanyId}", companyId);
                // Ensure we always have 5 color schemes
                if (config.ColorSchemes == null || config.ColorSchemes.Schemes == null || config.ColorSchemes.Schemes.Count < 5)
                {
                    _logger.LogInformation("Updating color schemes to have 5 schemes for company {CompanyId}", companyId);
                    config.ColorSchemes = GetDefaultColorSchemes();
                    config.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _cache.Set(cacheKey, config, CACHE_DURATION);
            return config;
        }

        public async Task<AppearanceConfig> GetAppearanceAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Appearance;
        }

        public async Task<TypographyConfig> GetTypographyAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Typography;
        }

        public async Task<ColorSchemesConfig> GetColorSchemesAsync(int companyId)
        {
            _logger.LogInformation("GetColorSchemesAsync called for company {CompanyId}", companyId);
            var config = await GetByCompanyIdAsync(companyId);
            
            if (config == null)
            {
                _logger.LogError("Config is null after GetByCompanyIdAsync for company {CompanyId}", companyId);
                throw new InvalidOperationException($"Failed to get or create config for company {companyId}");
            }
            
            if (config.ColorSchemes == null)
            {
                _logger.LogWarning("ColorSchemes is null for company {CompanyId}, returning default", companyId);
                return GetDefaultColorSchemes();
            }
            
            _logger.LogInformation("Returning {Count} color schemes for company {CompanyId}", 
                config.ColorSchemes?.Schemes?.Count ?? 0, companyId);
            return config.ColorSchemes;
        }

        public async Task<ProductCardsConfig> GetProductCardsAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.ProductCards;
        }

        public async Task<ProductBadgesConfig> GetProductBadgesAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.ProductBadges;
        }

        public async Task<CartConfig> GetCartAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Cart;
        }

        public async Task<FaviconConfig> GetFaviconAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Favicon;
        }

        public async Task<NavigationConfig> GetNavigationAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Navigation;
        }

        public async Task<SocialMediaConfig> GetSocialMediaAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.SocialMedia;
        }

        public async Task<SwatchesConfig> GetSwatchesAsync(int companyId)
        {
            var config = await GetByCompanyIdAsync(companyId);
            return config.Swatches;
        }

        public async Task<GlobalThemeConfig> UpdateAsync(int companyId, GlobalThemeConfig config)
        {
            var existingConfig = await _context.GlobalThemeConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (existingConfig == null)
            {
                throw new InvalidOperationException($"Theme config not found for company {companyId}");
            }

            // Update all modules
            existingConfig.Appearance = config.Appearance;
            existingConfig.Typography = config.Typography;
            existingConfig.ColorSchemes = config.ColorSchemes;
            existingConfig.ProductCards = config.ProductCards;
            existingConfig.ProductBadges = config.ProductBadges;
            existingConfig.Cart = config.Cart;
            existingConfig.Favicon = config.Favicon;
            existingConfig.Navigation = config.Navigation;
            existingConfig.SocialMedia = config.SocialMedia;
            existingConfig.Swatches = config.Swatches;
            existingConfig.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Invalidate cache
            _cache.Remove($"{CACHE_KEY_PREFIX}{companyId}");
            
            _logger.LogInformation("Theme config updated for company {CompanyId}", companyId);
            return existingConfig;
        }

        public async Task<AppearanceConfig> UpdateAppearanceAsync(int companyId, AppearanceConfig appearance)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Appearance = appearance;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Appearance config updated for company {CompanyId}", companyId);
            return appearance;
        }

        public async Task<TypographyConfig> UpdateTypographyAsync(int companyId, TypographyConfig typography)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Typography = typography;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Typography config updated for company {CompanyId}", companyId);
            return typography;
        }

        public async Task<ColorSchemesConfig> UpdateColorSchemesAsync(int companyId, ColorSchemesConfig colorSchemes)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.ColorSchemes = colorSchemes;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Color schemes updated for company {CompanyId}", companyId);
            return colorSchemes;
        }

        public async Task<ProductCardsConfig> UpdateProductCardsAsync(int companyId, ProductCardsConfig productCards)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.ProductCards = productCards;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Product cards config updated for company {CompanyId}", companyId);
            return productCards;
        }

        public async Task<ProductBadgesConfig> UpdateProductBadgesAsync(int companyId, ProductBadgesConfig productBadges)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.ProductBadges = productBadges;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Product badges config updated for company {CompanyId}", companyId);
            
            // Return the saved configuration from the database
            return config.ProductBadges;
        }

        public async Task<CartConfig> UpdateCartAsync(int companyId, CartConfig cart)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Cart = cart;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Cart config updated for company {CompanyId}", companyId);
            return cart;
        }

        public async Task<FaviconConfig> UpdateFaviconAsync(int companyId, FaviconConfig favicon)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Favicon = favicon;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Favicon config updated for company {CompanyId}", companyId);
            return favicon;
        }

        public async Task<NavigationConfig> UpdateNavigationAsync(int companyId, NavigationConfig navigation)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Navigation = navigation;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Navigation config updated for company {CompanyId}", companyId);
            return navigation;
        }

        public async Task<SocialMediaConfig> UpdateSocialMediaAsync(int companyId, SocialMediaConfig socialMedia)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.SocialMedia = socialMedia;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Social media config updated for company {CompanyId}", companyId);
            return socialMedia;
        }

        public async Task<SwatchesConfig> UpdateSwatchesAsync(int companyId, SwatchesConfig swatches)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.Swatches = swatches;
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Swatches config updated for company {CompanyId}", companyId);
            return swatches;
        }

        public async Task<bool> PublishAsync(int companyId)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            config.IsPublished = true;
            config.PublishedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Theme config published for company {CompanyId}", companyId);
            return true;
        }

        public async Task<GlobalThemeConfig> CreateDraftAsync(int companyId)
        {
            var currentConfig = await GetByCompanyIdAsync(companyId);
            
            // Create a copy with IsPublished = false
            var draft = new GlobalThemeConfig
            {
                CompanyId = companyId,
                Appearance = currentConfig.Appearance,
                Typography = currentConfig.Typography,
                ColorSchemes = currentConfig.ColorSchemes,
                ProductCards = currentConfig.ProductCards,
                ProductBadges = currentConfig.ProductBadges,
                Cart = currentConfig.Cart,
                Favicon = currentConfig.Favicon,
                Navigation = currentConfig.Navigation,
                SocialMedia = currentConfig.SocialMedia,
                Swatches = currentConfig.Swatches,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Draft theme config created for company {CompanyId}", companyId);
            return draft;
        }

        public async Task<bool> ResetModuleToDefaultAsync(int companyId, string moduleName)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            
            switch (moduleName.ToLower())
            {
                case "appearance":
                    config.Appearance = GetDefaultAppearance();
                    break;
                case "typography":
                    config.Typography = GetDefaultTypography();
                    break;
                case "colorschemes":
                    config.ColorSchemes = GetDefaultColorSchemes();
                    break;
                case "productcards":
                    config.ProductCards = GetDefaultProductCards();
                    break;
                case "productbadges":
                    config.ProductBadges = GetDefaultProductBadges();
                    break;
                case "cart":
                    config.Cart = GetDefaultCart();
                    break;
                case "favicon":
                    config.Favicon = GetDefaultFavicon();
                    break;
                case "navigation":
                    config.Navigation = GetDefaultNavigation();
                    break;
                case "socialmedia":
                    config.SocialMedia = GetDefaultSocialMedia();
                    break;
                case "swatches":
                    config.Swatches = GetDefaultSwatches();
                    break;
                default:
                    return false;
            }
            
            config.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("Module {ModuleName} reset to default for company {CompanyId}", moduleName, companyId);
            return true;
        }

        public async Task<GlobalThemeConfig> ResetAllToDefaultAsync(int companyId)
        {
            var config = await GetConfigForUpdateAsync(companyId);
            
            config.Appearance = GetDefaultAppearance();
            config.Typography = GetDefaultTypography();
            config.ColorSchemes = GetDefaultColorSchemes();
            config.ProductCards = GetDefaultProductCards();
            config.ProductBadges = GetDefaultProductBadges();
            config.Cart = GetDefaultCart();
            config.Favicon = GetDefaultFavicon();
            config.Navigation = GetDefaultNavigation();
            config.SocialMedia = GetDefaultSocialMedia();
            config.Swatches = GetDefaultSwatches();
            config.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            InvalidateCache(companyId);
            
            _logger.LogInformation("All theme config reset to default for company {CompanyId}", companyId);
            return config;
        }

        public async Task<bool> ValidateCompanyAccessAsync(int companyId, int userId)
        {
            // TODO: Implement proper permission checking
            var company = await _context.Companies.FindAsync(companyId);
            return company != null;
        }

        #region Private Helper Methods

        private async Task<GlobalThemeConfig> GetConfigForUpdateAsync(int companyId)
        {
            var config = await _context.GlobalThemeConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
                
            if (config == null)
            {
                throw new InvalidOperationException($"Theme config not found for company {companyId}");
            }
            
            return config;
        }

        private void InvalidateCache(int companyId)
        {
            _cache.Remove($"{CACHE_KEY_PREFIX}{companyId}");
        }

        private async Task<GlobalThemeConfig> CreateDefaultConfigAsync(int companyId)
        {
            try
            {
                // First check if company exists
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
                if (!companyExists)
                {
                    _logger.LogError("Cannot create GlobalThemeConfig - Company {CompanyId} does not exist", companyId);
                    throw new InvalidOperationException($"Company {companyId} does not exist");
                }
                
                var config = new GlobalThemeConfig
                {
                    CompanyId = companyId,
                    Appearance = GetDefaultAppearance(),
                    Typography = GetDefaultTypography(),
                    ColorSchemes = GetDefaultColorSchemes(),
                    ProductCards = GetDefaultProductCards(),
                    ProductBadges = GetDefaultProductBadges(),
                    Cart = GetDefaultCart(),
                    Favicon = GetDefaultFavicon(),
                    Navigation = GetDefaultNavigation(),
                    SocialMedia = GetDefaultSocialMedia(),
                    Swatches = GetDefaultSwatches(),
                    ConfigVersion = "2.0.0",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsPublished = false
                };

                _context.GlobalThemeConfigs.Add(config);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created default GlobalThemeConfig for company {CompanyId}", companyId);
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default GlobalThemeConfig for company {CompanyId}", companyId);
                throw;
            }
        }

        #region Default Configurations

        private AppearanceConfig GetDefaultAppearance() => new AppearanceConfig
        {
            PageWidth = 2300,
            LateralPadding = 31,
            BorderRadius = "size-2"
        };

        private TypographyConfig GetDefaultTypography() => new TypographyConfig
        {
            headings = new FontConfig 
            { 
                fontFamily = "Playfair Display", 
                fontSize = 100, 
                fontWeight = "400", 
                useUppercase = false, 
                letterSpacing = -1.7m 
            },
            body = new FontConfig 
            { 
                fontFamily = "Poppins", 
                fontSize = 15, 
                fontWeight = "400", 
                useUppercase = false, 
                letterSpacing = 0 
            },
            menu = new FontConfig 
            { 
                fontFamily = "Poppins", 
                fontSize = 94, 
                fontWeight = "400", 
                useUppercase = false, 
                letterSpacing = 0 
            },
            productCardName = new FontConfig 
            { 
                fontFamily = "Assistant", 
                fontSize = 100, 
                fontWeight = "400", 
                useUppercase = false, 
                letterSpacing = 0 
            },
            buttons = new FontConfig 
            { 
                fontFamily = "Assistant", 
                fontSize = 100, 
                fontWeight = "400", 
                useUppercase = false, 
                letterSpacing = 0 
            }
        };

        private ColorSchemesConfig GetDefaultColorSchemes() => new ColorSchemesConfig
        {
            DefaultScheme = "scheme-1",
            Schemes = new List<ColorScheme>
            {
                new ColorScheme
                {
                    Id = "scheme-1",
                    Name = "Scheme 1",
                    Text = "#000000",
                    Background = "#FFFFFF",
                    Foreground = "#F5E076",
                    Border = "#000000",
                    Link = "#F74703",
                    SolidButton = "#F74703",
                    SolidButtonText = "#FFFFFF",
                    OutlineButton = "#990F02",
                    OutlineButtonText = "#000000",
                    ImageOverlay = "rgba(0, 0, 0, 0.1)"
                },
                new ColorScheme
                {
                    Id = "scheme-2",
                    Name = "Scheme 2",
                    Text = "#FFFFFF",
                    Background = "#000000",
                    Foreground = "#333333",
                    Border = "#FFFFFF",
                    Link = "#66B2FF",
                    SolidButton = "#FFFFFF",
                    SolidButtonText = "#000000",
                    OutlineButton = "#FFFFFF",
                    OutlineButtonText = "#FFFFFF",
                    ImageOverlay = "rgba(255, 255, 255, 0.1)"
                },
                new ColorScheme
                {
                    Id = "scheme-3",
                    Name = "Scheme 3",
                    Text = "#2C3E50",
                    Background = "#ECF0F1",
                    Foreground = "#3498DB",
                    Border = "#BDC3C7",
                    Link = "#E74C3C",
                    SolidButton = "#E74C3C",
                    SolidButtonText = "#FFFFFF",
                    OutlineButton = "#2C3E50",
                    OutlineButtonText = "#2C3E50",
                    ImageOverlay = "rgba(52, 152, 219, 0.1)"
                },
                new ColorScheme
                {
                    Id = "scheme-4",
                    Name = "Scheme 4",
                    Text = "#F8F9FA",
                    Background = "#212529",
                    Foreground = "#6C757D",
                    Border = "#495057",
                    Link = "#FFC107",
                    SolidButton = "#FFC107",
                    SolidButtonText = "#212529",
                    OutlineButton = "#F8F9FA",
                    OutlineButtonText = "#F8F9FA",
                    ImageOverlay = "rgba(108, 117, 125, 0.2)"
                },
                new ColorScheme
                {
                    Id = "scheme-5",
                    Name = "Scheme 5",
                    Text = "#1A202C",
                    Background = "#F7FAFC",
                    Foreground = "#9AE6B4",
                    Border = "#CBD5E0",
                    Link = "#3182CE",
                    SolidButton = "#48BB78",
                    SolidButtonText = "#FFFFFF",
                    OutlineButton = "#3182CE",
                    OutlineButtonText = "#3182CE",
                    ImageOverlay = "rgba(154, 230, 180, 0.1)"
                }
            }
        };

        private ProductCardsConfig GetDefaultProductCards() => new ProductCardsConfig
        {
            Image = new ProductImageConfig { DefaultRatio = "square-1-1-fill" },
            Visibility = new ProductVisibilityConfig 
            { 
                ShowVendor = false,
                ShowCurrencyCode = true,
                ShowColorCount = true,
                ColorizeCardBackground = true,
                DarkenImageBackground = true
            },
            Rating = new ProductRatingConfig { ProductRating = "average-and-stars" },
            Price = new ProductPriceConfig { LabelSize = "large" },
            Effects = new ProductEffectsConfig { HoverEffect = "none" },
            Swatches = new ProductSwatchesConfig 
            { 
                WhatToShow = "color-swatches",
                ShowOnDesktop = "on-hover",
                ShowOnMobile = "always",
                HideForSingleColor = true
            },
            Buttons = new ProductButtonsConfig 
            { 
                QuickBuy = false,
                ShowQuickView = false,
                ShowAddToCart = false,
                DesktopStyle = "labels",
                ShowOnHover = true
            }
        };

        private ProductBadgesConfig GetDefaultProductBadges() => new ProductBadgesConfig
        {
            Position = new BadgePositionConfig
            {
                Desktop = "below-image"
            },
            SoldOut = new BadgeConfig 
            { 
                Enabled = true,
                Background = "#FFFFFF",
                Text = "#000000"
            },
            Sale = new BadgeConfig 
            { 
                Enabled = true,
                Background = "#FF0000",
                Text = "#FFFFFF",
                DisplayAs = "sale"
            },
            SaleByPrice = new BadgeConfig 
            { 
                Enabled = false,
                Background = "#000000",
                Text = "#FFFFFF",
                DisplayAs = "percentage"
            },
            SaleHighlight = new SaleHighlightConfig
            {
                Enabled = false,
                TextColor = "#000000"
            },
            Custom1 = new BadgeConfig
            {
                Enabled = false,
                Background = "#FFFFFF",
                Text = "#000000",
                TextContent = "Best seller",
                Tag = ""
            },
            Custom2 = new BadgeConfig
            {
                Enabled = false,
                Background = "#FFFFFF",
                Text = "#000000",
                TextContent = "",
                Tag = ""
            },
            Custom3 = new BadgeConfig
            {
                Enabled = false,
                Background = "#FFFFFF",
                Text = "#000000",
                TextContent = "",
                Tag = ""
            }
        };

        private CartConfig GetDefaultCart() => new CartConfig
        {
            DrawerType = "drawer-and-page",
            ShowStickyCart = false,
            CartStatusColors = new CartStatusColorsConfig
            {
                Background = "#F0FF2E",
                Text = "#383933"
            },
            FreeShipping = new FreeShippingConfig
            {
                ShowProgress = true,
                Threshold = 50.00m,
                ProgressBarColor = "#383933",
                SuccessMessage = "¡Envío gratis conseguido!",
                ProgressMessage = "Te faltan {amount} para envío gratis"
            }
        };

        private FaviconConfig GetDefaultFavicon() => new FaviconConfig
        {
            CustomFavicon = true,
            FaviconUrl = "/favicon-custom.ico"
        };

        private NavigationConfig GetDefaultNavigation() => new NavigationConfig
        {
            Search = new SearchConfig { ShowAs = "drawer-and-page" },
            BackToTop = new BackToTopConfig { ShowButton = true, Position = "bottom-left" }
        };

        private SocialMediaConfig GetDefaultSocialMedia() => new SocialMediaConfig
        {
            Facebook = "https://facebook.com/jacarandaplaza",
            Instagram = "https://instagram.com/jacarandaplaza"
        };

        private SwatchesConfig GetDefaultSwatches() => new SwatchesConfig
        {
            Primary = new PrimarySwatchConfig
            {
                Enabled = false,
                OptionName = "Color",
                ShapeForProductCards = "Portrait",
                SizeForProductCards = 3,
                ShapeForProductPages = "Round",
                SizeForProductPages = 4,
                ShapeForFilters = "Square",
                SizeForFilters = 1,
                CustomColorsAndPatterns = @"Ultramarine: #0437F2
Cherry blossom: #FFB7C5
Sunny day: yellow/green/blue
Summertime: #F8FBF8/#F8058/#4"
            },
            Secondary = new SecondarySwatchConfig
            {
                OptionNames = "Material\nFrame",
                ShapeForProductPages = "Square",
                SizeForProductPages = 4,
                ShapeForFilters = "Square",
                SizeForFilters = 1,
                CustomColorsAndPatterns = @"Ultramarine: #0437F2
Cherry blossom: #FFB7C5
Sunny day: yellow/green/blue
Summertime: #F8FBF8/#F8058/#4"
            }
        };

        #endregion

        #endregion
    }
}