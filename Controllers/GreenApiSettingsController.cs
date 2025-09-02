using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Common;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Manage GreenAPI settings-specific actions (rotate webhook token)
    /// </summary>
    [ApiController]
    [Route("api/whatsapp/greenapi")] 
    [Authorize]
    public class GreenApiSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GreenApiSettingsController> _logger;

        public GreenApiSettingsController(ApplicationDbContext context, ILogger<GreenApiSettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Rotate the webhook token and update the URL accordingly.
        /// </summary>
        [HttpPost("settings/rotate-webhook-token")]
        public async Task<IActionResult> RotateWebhookToken()
        {
            try
            {
                // CompanyId should come from auth claims; fallback to 1 if not present
                var companyId = GetCompanyId();

                var config = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Provider == "GreenAPI");

                if (config == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "GreenAPI configuration not found"
                    });
                }

                var newToken = GenerateToken();
                config.WebhookToken = newToken;
                config.WebhookUrl = $"/webhooks/greenapi/{companyId}/{newToken}";
                config.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Webhook token rotated successfully",
                    Data = new { webhookUrl = config.WebhookUrl }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate webhook token");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to rotate webhook token"
                });
            }
        }

        private int GetCompanyId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => c.Type == "company_id" || c.Type.EndsWith("/company_id"));
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            // Fallback for environments without auth wired during development
            return 1;
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');
        }
    }
}

