using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.Company;
using WebsiteBuilderAPI.DTOs.CheckoutSettings;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        /// <summary>
        /// Public company config used by frontend (no auth)
        /// </summary>
        [HttpGet("config")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicConfig()
        {
            try
            {
                // Use resilient service that returns safe defaults on DB issues
                var config = await _companyService.GetCompanyConfigAsync();
                if (config == null)
                {
                    // Extra safety: return hardcoded defaults so login/register never break
                    return Ok(new
                    {
                        id = 1,
                        name = "WebsiteBuilder",
                        currency = "USD",
                        logo = (string?)null,
                        colors = new { primary = "#22c55e", secondary = "#64748b" },
                        contact = new { email = (string?)null, phone = (string?)null },
                        domain = (string?)null
                    });
                }

                return Ok(new
                {
                    id = 1,
                    name = config.Name,
                    currency = config.Currency ?? "USD",
                    logo = config.Logo,
                    colors = new { primary = config.PrimaryColor ?? "#22c55e", secondary = config.SecondaryColor ?? "#64748b" },
                    contact = new { email = (string?)null, phone = (string?)null },
                    domain = (string?)null
                });
            }
            catch (Exception ex)
            {
                // Final safeguard: never let this endpoint fail hard in public pages
                _logger.LogError(ex, "Error getting public company config - returning defaults");
                return Ok(new
                {
                    id = 1,
                    name = "WebsiteBuilder",
                    currency = "USD",
                    logo = (string?)null,
                    colors = new { primary = "#22c55e", secondary = "#64748b" },
                    contact = new { email = (string?)null, phone = (string?)null },
                    domain = (string?)null
                });
            }
        }

        /// <summary>
        /// Obtiene la información del company actual (mi empresa)
        /// </summary>
        [HttpGet("current")]
        [RequirePermission("company", "read")]
        public async Task<ActionResult<CompanyResponseDto>> GetCurrentCompany()
        {
            try
            {
                var company = await _companyService.GetCurrentCompanyAsync();
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current company");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene información básica de la empresa (público para preview)
        /// </summary>
        [HttpGet("{companyId}/public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicCompanyInfo(int companyId)
        {
            try
            {
                var company = await _companyService.GetCompanyEntityByIdAsync(companyId);
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                // Return only basic public info
                // Include safe geolocation data for public maps (Mapbox public tokens only)
                string? geoProvider = company.GeolocationProvider;
                string? geoPublicToken = null;
                if (!string.IsNullOrEmpty(geoProvider) && geoProvider.Equals("mapbox", StringComparison.OrdinalIgnoreCase))
                {
                    var token = company.GeolocationToken ?? string.Empty;
                    // Only expose public Mapbox tokens (pk.*). Never expose secret tokens (sk.*)
                    if (token.StartsWith("pk."))
                    {
                        geoPublicToken = token;
                    }
                }

                return Ok(new
                {
                    id = company.Id,
                    name = company.Name,
                    currency = company.Currency ?? company.CurrencyBase ?? "USD",
                    logo = company.Logo,
                    primaryColor = company.PrimaryColor,
                    secondaryColor = company.SecondaryColor,
                    address = company.Address,
                    city = company.City,
                    state = company.State,
                    postalCode = company.PostalCode,
                    phone = company.PhoneNumber,
                    email = company.ContactEmail,
                    geolocationProvider = geoProvider,
                    geolocationPublicToken = geoPublicToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public company info");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza la información del company actual
        /// </summary>
        [HttpPut("current")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult<CompanyResponseDto>> UpdateCurrentCompany([FromBody] UpdateCompanyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCompany = await _companyService.UpdateCurrentCompanyAsync(request);
                if (updatedCompany == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                return Ok(updatedCompany);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current company");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza solo el logo del company
        /// </summary>
        [HttpPut("current/logo")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult> UpdateLogo([FromBody] UpdateLogoDto request)
        {
            try
            {
                await _companyService.UpdateLogoAsync(request.Logo);
                return Ok(new { message = "Logo updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating logo");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Sube el logo del company
        /// </summary>
        [HttpPost("current/logo")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult<string>> UploadLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                // Validar tipo de archivo
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPEG, PNG and GIF are allowed." });
                }

                // Validar tamaño (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File too large. Maximum size is 5MB." });
                }

                var logoUrl = await _companyService.UploadLogoAsync(file);
                return Ok(new { logoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza el tamaño del logo del company
        /// </summary>
        [HttpPut("current/logo-size")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult> UpdateLogoSize([FromBody] UpdateLogoSizeDto request)
        {
            try
            {
                if (request.Size < 60 || request.Size > 200)
                {
                    return BadRequest(new { message = "Logo size must be between 60 and 200 pixels" });
                }

                await _companyService.UpdateLogoSizeAsync(request.Size);
                return Ok(new { message = "Logo size updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating logo size");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        #region Checkout Settings

        /// <summary>
        /// Obtiene la configuración de checkout del company
        /// </summary>
        [HttpGet("checkout-settings")]
        [RequirePermission("company", "read")]
        public async Task<ActionResult<CheckoutSettingsDto>> GetCheckoutSettings()
        {
            try
            {
                var settings = await _companyService.GetCheckoutSettingsAsync();
                if (settings == null)
                {
                    // Si no existe, devolver configuración por defecto
                    settings = await _companyService.CreateDefaultCheckoutSettingsAsync();
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout settings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza la configuración de checkout del company
        /// </summary>
        [HttpPut("checkout-settings")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult<CheckoutSettingsDto>> UpdateCheckoutSettings([FromBody] UpdateCheckoutSettingsDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedSettings = await _companyService.UpdateCheckoutSettingsAsync(request);
                return Ok(updatedSettings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checkout settings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Restablece la configuración de checkout a los valores por defecto
        /// </summary>
        [HttpPost("checkout-settings/reset")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult<CheckoutSettingsDto>> ResetCheckoutSettings()
        {
            try
            {
                var defaultSettings = await _companyService.ResetCheckoutSettingsAsync();
                return Ok(defaultSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting checkout settings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload checkout-specific logo (used on checkout header)
        /// </summary>
        [HttpPost("checkout-settings/logo")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult> UploadCheckoutLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/svg+xml" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, GIF, SVG are allowed." });
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File too large. Maximum size is 5MB." });
                }

                var logoUrl = await _companyService.UploadCheckoutLogoAsync(file);
                return Ok(new { logoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading checkout logo");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Currency Settings (Manual)

        /// <summary>
        /// Obtiene configuración de moneda/tasas (manual) para una empresa
        /// </summary>
        [HttpGet("/api/currency/company/{companyId}/effective-settings")]
        [AllowAnonymous]
        public async Task<ActionResult<CurrencySettingsDto>> GetCurrencySettings([FromRoute] int companyId)
        {
            try
            {
                var settings = await _companyService.GetCurrencySettingsAsync(companyId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting currency settings for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza configuración de moneda/tasas (manual) para una empresa
        /// </summary>
        [HttpPut("{companyId}/currency-settings")]
        [RequirePermission("company", "update")]
        public async Task<ActionResult<CurrencySettingsDto>> UpdateCurrencySettings([FromRoute] int companyId, [FromBody] CurrencySettingsDto request)
        {
            try
            {
                await _companyService.UpdateCurrencySettingsAsync(companyId, request);
                // Return the updated settings
                var updatedSettings = await _companyService.GetCurrencySettingsAsync(companyId);
                return Ok(updatedSettings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency settings for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion
    }
}
