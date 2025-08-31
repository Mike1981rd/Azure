using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebsiteBuilderAPI.Models.ThemeConfig;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// API Controller for managing global theme configurations
    /// Implements modular endpoints to avoid transferring large JSON payloads
    /// </summary>
    [ApiController]
    [Route("api/global-theme-config")]
    [Authorize]
    public class GlobalThemeConfigController : ControllerBase
    {
        private readonly IGlobalThemeConfigService _themeService;
        private readonly ILogger<GlobalThemeConfigController> _logger;

        public GlobalThemeConfigController(
            IGlobalThemeConfigService themeService,
            ILogger<GlobalThemeConfigController> logger)
        {
            _themeService = themeService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the complete theme configuration for a company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Complete theme configuration</returns>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<GlobalThemeConfig>> GetByCompanyId(int companyId)
        {
            try
            {
                var config = await _themeService.GetByCompanyIdAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving theme configuration" });
            }
        }

        /// <summary>
        /// Gets the published theme configuration for a company (public endpoint for preview)
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Complete theme configuration</returns>
        [HttpGet("company/{companyId}/published")]
        [AllowAnonymous]
        public async Task<ActionResult<GlobalThemeConfig>> GetPublishedByCompanyId(int companyId)
        {
            try
            {
                var config = await _themeService.GetByCompanyIdAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published theme config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving theme configuration" });
            }
        }

        #region Individual Module GET Endpoints

        /// <summary>
        /// Gets only the appearance configuration
        /// </summary>
        [HttpGet("company/{companyId}/appearance")]
        public async Task<ActionResult<AppearanceConfig>> GetAppearance(int companyId)
        {
            try
            {
                var config = await _themeService.GetAppearanceAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appearance config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving appearance configuration" });
            }
        }

        /// <summary>
        /// Gets only the typography configuration
        /// </summary>
        [HttpGet("company/{companyId}/typography")]
        public async Task<ActionResult<TypographyConfig>> GetTypography(int companyId)
        {
            try
            {
                var config = await _themeService.GetTypographyAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting typography config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving typography configuration" });
            }
        }

        /// <summary>
        /// Gets only the color schemes configuration
        /// </summary>
        [HttpGet("company/{companyId}/color-schemes")]
        public async Task<ActionResult<ColorSchemesConfig>> GetColorSchemes(int companyId)
        {
            try
            {
                _logger.LogInformation("Getting color schemes for company {CompanyId}", companyId);
                var config = await _themeService.GetColorSchemesAsync(companyId);
                _logger.LogInformation("Successfully retrieved color schemes for company {CompanyId}", companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting color schemes for company {CompanyId}: {Message}", companyId, ex.Message);
                return StatusCode(500, new { message = $"Error retrieving color schemes: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets only the product cards configuration
        /// </summary>
        [HttpGet("company/{companyId}/product-cards")]
        public async Task<ActionResult<ProductCardsConfig>> GetProductCards(int companyId)
        {
            try
            {
                var config = await _themeService.GetProductCardsAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product cards config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving product cards configuration" });
            }
        }

        /// <summary>
        /// Gets only the product badges configuration
        /// </summary>
        [HttpGet("company/{companyId}/product-badges")]
        public async Task<ActionResult<ProductBadgesConfig>> GetProductBadges(int companyId)
        {
            try
            {
                var config = await _themeService.GetProductBadgesAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product badges config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving product badges configuration" });
            }
        }

        /// <summary>
        /// Gets only the cart configuration
        /// </summary>
        [HttpGet("company/{companyId}/cart")]
        public async Task<ActionResult<CartConfig>> GetCart(int companyId)
        {
            try
            {
                var config = await _themeService.GetCartAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving cart configuration" });
            }
        }

        /// <summary>
        /// Gets only the favicon configuration
        /// </summary>
        [HttpGet("company/{companyId}/favicon")]
        public async Task<ActionResult<FaviconConfig>> GetFavicon(int companyId)
        {
            try
            {
                var config = await _themeService.GetFaviconAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favicon config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving favicon configuration" });
            }
        }

        /// <summary>
        /// Gets only the navigation configuration
        /// </summary>
        [HttpGet("company/{companyId}/navigation")]
        public async Task<ActionResult<NavigationConfig>> GetNavigation(int companyId)
        {
            try
            {
                var config = await _themeService.GetNavigationAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving navigation configuration" });
            }
        }

        /// <summary>
        /// Gets only the social media configuration
        /// </summary>
        [HttpGet("company/{companyId}/social-media")]
        public async Task<ActionResult<SocialMediaConfig>> GetSocialMedia(int companyId)
        {
            try
            {
                var config = await _themeService.GetSocialMediaAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting social media config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving social media configuration" });
            }
        }

        /// <summary>
        /// Gets only the swatches configuration
        /// </summary>
        [HttpGet("company/{companyId}/swatches")]
        public async Task<ActionResult<SwatchesConfig>> GetSwatches(int companyId)
        {
            try
            {
                var config = await _themeService.GetSwatchesAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting swatches config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving swatches configuration" });
            }
        }

        #endregion

        #region Update Endpoints

        /// <summary>
        /// Updates the complete theme configuration
        /// </summary>
        [HttpPut("company/{companyId}")]
        public async Task<ActionResult<GlobalThemeConfig>> UpdateComplete(int companyId, [FromBody] GlobalThemeConfig config)
        {
            try
            {
                var updated = await _themeService.UpdateAsync(companyId, config);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating theme configuration" });
            }
        }

        /// <summary>
        /// Updates only the appearance module
        /// </summary>
        [HttpPatch("company/{companyId}/appearance")]
        public async Task<ActionResult<AppearanceConfig>> UpdateAppearance(int companyId, [FromBody] AppearanceConfig appearance)
        {
            try
            {
                var updated = await _themeService.UpdateAppearanceAsync(companyId, appearance);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appearance for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating appearance configuration" });
            }
        }

        /// <summary>
        /// Updates only the typography module
        /// </summary>
        [HttpPatch("company/{companyId}/typography")]
        public async Task<ActionResult<TypographyConfig>> UpdateTypography(int companyId, [FromBody] TypographyConfig typography)
        {
            try
            {
                var updated = await _themeService.UpdateTypographyAsync(companyId, typography);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating typography for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating typography configuration" });
            }
        }

        /// <summary>
        /// Updates only the color schemes module
        /// </summary>
        [HttpPatch("company/{companyId}/color-schemes")]
        public async Task<ActionResult<ColorSchemesConfig>> UpdateColorSchemes(int companyId, [FromBody] ColorSchemesConfig colorSchemes)
        {
            try
            {
                var updated = await _themeService.UpdateColorSchemesAsync(companyId, colorSchemes);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating color schemes for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating color schemes" });
            }
        }

        /// <summary>
        /// Updates only the product cards module
        /// </summary>
        [HttpPatch("company/{companyId}/product-cards")]
        public async Task<ActionResult<ProductCardsConfig>> UpdateProductCards(int companyId, [FromBody] ProductCardsConfig productCards)
        {
            try
            {
                var updated = await _themeService.UpdateProductCardsAsync(companyId, productCards);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product cards for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating product cards configuration" });
            }
        }

        /// <summary>
        /// Updates only the product badges module
        /// </summary>
        [HttpPatch("company/{companyId}/product-badges")]
        public async Task<ActionResult<ProductBadgesConfig>> UpdateProductBadges(int companyId, [FromBody] ProductBadgesConfig productBadges)
        {
            try
            {
                var updated = await _themeService.UpdateProductBadgesAsync(companyId, productBadges);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product badges for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating product badges configuration" });
            }
        }

        /// <summary>
        /// Updates only the cart module
        /// </summary>
        [HttpPatch("company/{companyId}/cart")]
        public async Task<ActionResult<CartConfig>> UpdateCart(int companyId, [FromBody] CartConfig cart)
        {
            try
            {
                var updated = await _themeService.UpdateCartAsync(companyId, cart);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating cart configuration" });
            }
        }

        /// <summary>
        /// Updates only the favicon module
        /// </summary>
        [HttpPatch("company/{companyId}/favicon")]
        public async Task<ActionResult<FaviconConfig>> UpdateFavicon(int companyId, [FromBody] FaviconConfig favicon)
        {
            try
            {
                var updated = await _themeService.UpdateFaviconAsync(companyId, favicon);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating favicon for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating favicon configuration" });
            }
        }

        /// <summary>
        /// Updates only the navigation module
        /// </summary>
        [HttpPatch("company/{companyId}/navigation")]
        public async Task<ActionResult<NavigationConfig>> UpdateNavigation(int companyId, [FromBody] NavigationConfig navigation)
        {
            try
            {
                var updated = await _themeService.UpdateNavigationAsync(companyId, navigation);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating navigation for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating navigation configuration" });
            }
        }

        /// <summary>
        /// Updates only the social media module
        /// </summary>
        [HttpPatch("company/{companyId}/social-media")]
        public async Task<ActionResult<SocialMediaConfig>> UpdateSocialMedia(int companyId, [FromBody] SocialMediaConfig socialMedia)
        {
            try
            {
                // Log incoming data for debugging
                _logger.LogInformation("Received social media update for company {CompanyId}. Data: {@SocialMedia}", companyId, socialMedia);
                
                // Check if model state is valid
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for social media update: {@Errors}", ModelState);
                    return BadRequest(ModelState);
                }
                
                var updated = await _themeService.UpdateSocialMediaAsync(companyId, socialMedia);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating social media for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating social media configuration" });
            }
        }

        /// <summary>
        /// Updates only the swatches module
        /// </summary>
        [HttpPatch("company/{companyId}/swatches")]
        public async Task<ActionResult<SwatchesConfig>> UpdateSwatches(int companyId, [FromBody] SwatchesConfig swatches)
        {
            try
            {
                var updated = await _themeService.UpdateSwatchesAsync(companyId, swatches);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating swatches for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error updating swatches configuration" });
            }
        }

        #endregion

        #region Special Operations

        /// <summary>
        /// Publishes the current configuration making it live
        /// </summary>
        [HttpPost("company/{companyId}/publish")]
        public async Task<ActionResult> Publish(int companyId)
        {
            try
            {
                var result = await _themeService.PublishAsync(companyId);
                if (result)
                {
                    return Ok(new { message = "Configuration published successfully" });
                }
                return BadRequest(new { message = "Failed to publish configuration" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error publishing configuration" });
            }
        }

        /// <summary>
        /// Creates a draft copy of the current configuration
        /// </summary>
        [HttpPost("company/{companyId}/create-draft")]
        public async Task<ActionResult<GlobalThemeConfig>> CreateDraft(int companyId)
        {
            try
            {
                var draft = await _themeService.CreateDraftAsync(companyId);
                return Ok(draft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating draft for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error creating draft" });
            }
        }

        /// <summary>
        /// Resets a specific module to default values
        /// </summary>
        [HttpPost("company/{companyId}/reset-module/{moduleName}")]
        public async Task<ActionResult> ResetModule(int companyId, string moduleName)
        {
            try
            {
                var result = await _themeService.ResetModuleToDefaultAsync(companyId, moduleName);
                if (result)
                {
                    return Ok(new { message = $"Module {moduleName} reset to default successfully" });
                }
                return BadRequest(new { message = $"Invalid module name: {moduleName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting module {ModuleName} for company {CompanyId}", moduleName, companyId);
                return StatusCode(500, new { message = "Error resetting module" });
            }
        }

        /// <summary>
        /// Resets all configuration to default values
        /// </summary>
        [HttpPost("company/{companyId}/reset-all")]
        public async Task<ActionResult<GlobalThemeConfig>> ResetAll(int companyId)
        {
            try
            {
                var config = await _themeService.ResetAllToDefaultAsync(companyId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting all config for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error resetting configuration" });
            }
        }

        #endregion
    }
}