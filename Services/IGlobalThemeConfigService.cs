using System.Threading.Tasks;
using WebsiteBuilderAPI.Models.ThemeConfig;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service interface for managing global theme configurations
    /// Implements the modular theme system v2.0 to avoid monolithic JSON
    /// </summary>
    public interface IGlobalThemeConfigService
    {
        /// <summary>
        /// Gets the complete theme configuration for a company
        /// Creates default if none exists
        /// </summary>
        Task<GlobalThemeConfig> GetByCompanyIdAsync(int companyId);

        /// <summary>
        /// Gets only the appearance configuration module
        /// </summary>
        Task<AppearanceConfig> GetAppearanceAsync(int companyId);

        /// <summary>
        /// Gets only the typography configuration module
        /// </summary>
        Task<TypographyConfig> GetTypographyAsync(int companyId);

        /// <summary>
        /// Gets only the color schemes configuration module
        /// </summary>
        Task<ColorSchemesConfig> GetColorSchemesAsync(int companyId);

        /// <summary>
        /// Gets only the product cards configuration module
        /// </summary>
        Task<ProductCardsConfig> GetProductCardsAsync(int companyId);

        /// <summary>
        /// Gets only the product badges configuration module
        /// </summary>
        Task<ProductBadgesConfig> GetProductBadgesAsync(int companyId);

        /// <summary>
        /// Gets only the cart configuration module
        /// </summary>
        Task<CartConfig> GetCartAsync(int companyId);

        /// <summary>
        /// Gets only the favicon configuration module
        /// </summary>
        Task<FaviconConfig> GetFaviconAsync(int companyId);

        /// <summary>
        /// Gets only the navigation configuration module
        /// </summary>
        Task<NavigationConfig> GetNavigationAsync(int companyId);

        /// <summary>
        /// Gets only the social media configuration module
        /// </summary>
        Task<SocialMediaConfig> GetSocialMediaAsync(int companyId);

        /// <summary>
        /// Gets only the swatches configuration module
        /// </summary>
        Task<SwatchesConfig> GetSwatchesAsync(int companyId);

        /// <summary>
        /// Updates the complete theme configuration
        /// </summary>
        Task<GlobalThemeConfig> UpdateAsync(int companyId, GlobalThemeConfig config);

        /// <summary>
        /// Updates only the appearance module
        /// </summary>
        Task<AppearanceConfig> UpdateAppearanceAsync(int companyId, AppearanceConfig appearance);

        /// <summary>
        /// Updates only the typography module
        /// </summary>
        Task<TypographyConfig> UpdateTypographyAsync(int companyId, TypographyConfig typography);

        /// <summary>
        /// Updates only the color schemes module
        /// </summary>
        Task<ColorSchemesConfig> UpdateColorSchemesAsync(int companyId, ColorSchemesConfig colorSchemes);

        /// <summary>
        /// Updates only the product cards module
        /// </summary>
        Task<ProductCardsConfig> UpdateProductCardsAsync(int companyId, ProductCardsConfig productCards);

        /// <summary>
        /// Updates only the product badges module
        /// </summary>
        Task<ProductBadgesConfig> UpdateProductBadgesAsync(int companyId, ProductBadgesConfig productBadges);

        /// <summary>
        /// Updates only the cart module
        /// </summary>
        Task<CartConfig> UpdateCartAsync(int companyId, CartConfig cart);

        /// <summary>
        /// Updates only the favicon module
        /// </summary>
        Task<FaviconConfig> UpdateFaviconAsync(int companyId, FaviconConfig favicon);

        /// <summary>
        /// Updates only the navigation module
        /// </summary>
        Task<NavigationConfig> UpdateNavigationAsync(int companyId, NavigationConfig navigation);

        /// <summary>
        /// Updates only the social media module
        /// </summary>
        Task<SocialMediaConfig> UpdateSocialMediaAsync(int companyId, SocialMediaConfig socialMedia);

        /// <summary>
        /// Updates only the swatches module
        /// </summary>
        Task<SwatchesConfig> UpdateSwatchesAsync(int companyId, SwatchesConfig swatches);

        /// <summary>
        /// Publishes the current configuration making it live
        /// </summary>
        Task<bool> PublishAsync(int companyId);

        /// <summary>
        /// Creates a copy of the current configuration for editing
        /// </summary>
        Task<GlobalThemeConfig> CreateDraftAsync(int companyId);

        /// <summary>
        /// Resets a specific module to default values
        /// </summary>
        Task<bool> ResetModuleToDefaultAsync(int companyId, string moduleName);

        /// <summary>
        /// Resets all configuration to default values
        /// </summary>
        Task<GlobalThemeConfig> ResetAllToDefaultAsync(int companyId);

        /// <summary>
        /// Validates if a company exists and has permissions
        /// </summary>
        Task<bool> ValidateCompanyAccessAsync(int companyId, int userId);
    }
}