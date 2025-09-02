using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// WhatsApp Testing and Diagnostics Controller
    /// Provides endpoints for testing WhatsApp functionality with multiple providers
    /// </summary>
    [ApiController]
    [Route("api/whatsapp/test")]
    [Authorize]
    [SwaggerTag("WhatsApp Testing - Test and diagnose WhatsApp integrations")]
    public class WhatsAppTestController : ControllerBase
    {
        private readonly IWhatsAppServiceFactory _whatsAppServiceFactory;
        private readonly ILogger<WhatsAppTestController> _logger;

        public WhatsAppTestController(
            IWhatsAppServiceFactory whatsAppServiceFactory,
            ILogger<WhatsAppTestController> logger)
        {
            _whatsAppServiceFactory = whatsAppServiceFactory ?? throw new ArgumentNullException(nameof(whatsAppServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get current WhatsApp provider information
        /// </summary>
        /// <returns>Current provider details and status</returns>
        [AllowAnonymous] // Allow anonymous access for basic provider info
        [HttpGet("current-provider")]
        [SwaggerOperation(Summary = "Get current WhatsApp provider", Description = "Returns information about the currently active WhatsApp provider")]
        [SwaggerResponse(200, "Current provider information", typeof(WhatsAppProviderInfoDto))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<WhatsAppProviderInfoDto>> GetCurrentProvider()
        {
            try
            {
                _logger.LogInformation("Getting current WhatsApp provider information");

                var activeProvider = _whatsAppServiceFactory.GetActiveProvider();
                var availableProviders = _whatsAppServiceFactory.GetAvailableProviders();
                var service = _whatsAppServiceFactory.GetService();

                // Get company ID from claims (assuming it's available in JWT)
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                {
                    companyId = 1; // Default for testing
                }

                var isConfigured = service.IsConfigured(companyId);

                var result = new WhatsAppProviderInfoDto
                {
                    ActiveProvider = activeProvider,
                    ProviderName = service.ProviderName,
                    AvailableProviders = availableProviders,
                    IsConfigured = isConfigured,
                    CompanyId = companyId,
                    CheckedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Current WhatsApp provider: {Provider}, Configured: {IsConfigured}", 
                    activeProvider, isConfigured);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current WhatsApp provider information");
                return StatusCode(500, new { message = "Error getting provider information", error = ex.Message });
            }
        }

        /// <summary>
        /// Send a test message using the current WhatsApp provider
        /// </summary>
        /// <param name="request">Test message request</param>
        /// <returns>Test message result</returns>
        [HttpPost("send-test-message")]
        [SwaggerOperation(Summary = "Send test WhatsApp message", Description = "Sends a test message using the current WhatsApp provider")]
        [SwaggerResponse(200, "Test message sent successfully", typeof(WhatsAppTestMessageResultDto))]
        [SwaggerResponse(400, "Bad request")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<WhatsAppTestMessageResultDto>> SendTestMessage([FromBody] WhatsAppTestMessageRequestDto request)
        {
            try
            {
                _logger.LogInformation("Sending test WhatsApp message to {PhoneNumber}", request.PhoneNumber);

                // Get company ID from claims
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                {
                    companyId = 1; // Default for testing
                }

                var service = _whatsAppServiceFactory.GetService();

                // Check if service is configured
                if (!service.IsConfigured(companyId))
                {
                    return BadRequest(new { message = $"WhatsApp provider {service.ProviderName} is not configured for company {companyId}" });
                }

                // Send test message
                var sendDto = new SendWhatsAppMessageDto
                {
                    To = request.PhoneNumber,
                    Body = request.Message ?? $"Test message from WebsiteBuilder API using {service.ProviderName} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                };

                var startTime = DateTime.UtcNow;
                var messageResult = await service.SendMessageAsync(companyId, sendDto);
                var endTime = DateTime.UtcNow;

                var result = new WhatsAppTestMessageResultDto
                {
                    Success = true,
                    Provider = service.ProviderName,
                    MessageId = messageResult.TwilioSid,
                    PhoneNumber = request.PhoneNumber,
                    MessageContent = sendDto.Body,
                    SentAt = messageResult.Timestamp,
                    DurationMs = (long)(endTime - startTime).TotalMilliseconds,
                    Status = messageResult.Status
                };

                _logger.LogInformation("Test message sent successfully via {Provider}. MessageId: {MessageId}", 
                    service.ProviderName, messageResult.TwilioSid);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test WhatsApp message to {PhoneNumber}", request.PhoneNumber);

                var errorResult = new WhatsAppTestMessageResultDto
                {
                    Success = false,
                    Provider = _whatsAppServiceFactory.GetActiveProvider(),
                    PhoneNumber = request.PhoneNumber,
                    MessageContent = request.Message,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow
                };

                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Switch to a different WhatsApp provider (for testing purposes)
        /// </summary>
        /// <param name="request">Provider switch request</param>
        /// <returns>Switch result</returns>
        [HttpPost("switch-provider")]
        [SwaggerOperation(Summary = "Switch WhatsApp provider", Description = "Switches to a different WhatsApp provider for testing")]
        [SwaggerResponse(200, "Provider switched successfully", typeof(WhatsAppProviderSwitchResultDto))]
        [SwaggerResponse(400, "Bad request")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<WhatsAppProviderSwitchResultDto>> SwitchProvider([FromBody] WhatsAppProviderSwitchRequestDto request)
        {
            try
            {
                _logger.LogInformation("Switching WhatsApp provider to {ProviderName}", request.ProviderName);

                var availableProviders = _whatsAppServiceFactory.GetAvailableProviders();
                var currentProvider = _whatsAppServiceFactory.GetActiveProvider();

                if (!availableProviders.Contains(request.ProviderName, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { 
                        message = $"Provider '{request.ProviderName}' is not available", 
                        availableProviders = availableProviders 
                    });
                }

                // Get the service for the requested provider
                var service = _whatsAppServiceFactory.GetService(request.ProviderName);

                // Get company ID from claims
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                {
                    companyId = 1; // Default for testing
                }

                var isConfigured = service.IsConfigured(companyId);

                var result = new WhatsAppProviderSwitchResultDto
                {
                    Success = true,
                    PreviousProvider = currentProvider,
                    NewProvider = service.ProviderName,
                    IsConfigured = isConfigured,
                    SwitchedAt = DateTime.UtcNow,
                    CompanyId = companyId,
                    Message = $"Successfully switched to {service.ProviderName}. " + 
                             (isConfigured ? "Provider is configured and ready to use." : "Provider needs configuration.")
                };

                _logger.LogInformation("WhatsApp provider switched from {PreviousProvider} to {NewProvider}", 
                    currentProvider, service.ProviderName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching WhatsApp provider to {ProviderName}", request.ProviderName);

                var errorResult = new WhatsAppProviderSwitchResultDto
                {
                    Success = false,
                    PreviousProvider = _whatsAppServiceFactory.GetActiveProvider(),
                    NewProvider = request.ProviderName,
                    ErrorMessage = ex.Message,
                    SwitchedAt = DateTime.UtcNow
                };

                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Test WhatsApp configuration for a specific provider
        /// </summary>
        /// <param name="providerName">Provider to test (Twilio or GreenAPI)</param>
        /// <param name="request">Test configuration request</param>
        /// <returns>Configuration test result</returns>
        [HttpPost("test-config/{providerName}")]
        [SwaggerOperation(Summary = "Test WhatsApp provider configuration", Description = "Tests the configuration of a specific WhatsApp provider")]
        [SwaggerResponse(200, "Configuration test completed", typeof(WhatsAppConfigTestResultDto))]
        [SwaggerResponse(400, "Bad request")]
        [SwaggerResponse(404, "Provider not found")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<WhatsAppConfigTestResultDto>> TestConfig(
            string providerName, 
            [FromBody] TestWhatsAppConfigDto request)
        {
            try
            {
                _logger.LogInformation("Testing WhatsApp configuration for provider {ProviderName}", providerName);

                var availableProviders = _whatsAppServiceFactory.GetAvailableProviders();
                if (!availableProviders.Contains(providerName, StringComparer.OrdinalIgnoreCase))
                {
                    return NotFound(new { 
                        message = $"Provider '{providerName}' not found", 
                        availableProviders = availableProviders 
                    });
                }

                // Get company ID from claims
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                {
                    companyId = 1; // Default for testing
                }

                var service = _whatsAppServiceFactory.GetService(providerName);
                var result = await service.TestConfigAsync(companyId, request);

                _logger.LogInformation("Configuration test completed for {Provider}. Success: {Success}", 
                    providerName, result.Success);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing WhatsApp configuration for provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error testing configuration", error = ex.Message });
            }
        }

        /// <summary>
        /// Get health status of all WhatsApp providers
        /// </summary>
        /// <returns>Health status of all providers</returns>
        [HttpGet("health")]
        [SwaggerOperation(Summary = "Get WhatsApp providers health status", Description = "Returns health status of all available WhatsApp providers")]
        [SwaggerResponse(200, "Health status retrieved", typeof(WhatsAppProvidersHealthDto))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<WhatsAppProvidersHealthDto>> GetHealth()
        {
            try
            {
                _logger.LogInformation("Getting WhatsApp providers health status");

                var availableProviders = _whatsAppServiceFactory.GetAvailableProviders();
                var activeProvider = _whatsAppServiceFactory.GetActiveProvider();

                // Get company ID from claims
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                {
                    companyId = 1; // Default for testing
                }

                var providerStatuses = new List<WhatsAppProviderHealthDto>();

                foreach (var providerName in availableProviders)
                {
                    try
                    {
                        var service = _whatsAppServiceFactory.GetService(providerName);
                        var isConfigured = service.IsConfigured(companyId);
                        var isActive = providerName.Equals(activeProvider, StringComparison.OrdinalIgnoreCase);

                        providerStatuses.Add(new WhatsAppProviderHealthDto
                        {
                            ProviderName = service.ProviderName,
                            IsActive = isActive,
                            IsConfigured = isConfigured,
                            Status = isConfigured ? "Healthy" : "Not Configured",
                            LastChecked = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking health for provider {ProviderName}", providerName);
                        providerStatuses.Add(new WhatsAppProviderHealthDto
                        {
                            ProviderName = providerName,
                            IsActive = false,
                            IsConfigured = false,
                            Status = "Error",
                            ErrorMessage = ex.Message,
                            LastChecked = DateTime.UtcNow
                        });
                    }
                }

                var result = new WhatsAppProvidersHealthDto
                {
                    OverallStatus = providerStatuses.Any(p => p.IsActive && p.IsConfigured) ? "Healthy" : "Degraded",
                    ActiveProvider = activeProvider,
                    CompanyId = companyId,
                    Providers = providerStatuses,
                    CheckedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WhatsApp providers health status");
                return StatusCode(500, new { message = "Error getting health status", error = ex.Message });
            }
        }
    }
}