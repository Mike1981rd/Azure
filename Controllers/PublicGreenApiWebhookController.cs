using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Common;
using WebsiteBuilderAPI.Services.Encryption;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Public webhook endpoint for GreenAPI with companyId + token routing and header validation
    /// </summary>
    [ApiController]
    [Route("webhooks/greenapi")]
    public class PublicGreenApiWebhookController : ControllerBase
    {
        private readonly ILogger<PublicGreenApiWebhookController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public PublicGreenApiWebhookController(
            ILogger<PublicGreenApiWebhookController> logger,
            ApplicationDbContext context,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _context = context;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Receives GreenAPI webhook and validates header + token.
        /// Path: /webhooks/greenapi/{companyId}/{token}
        /// </summary>
        [HttpPost("{companyId:int}/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> Receive(int companyId, string token, [FromBody] JObject payload)
        {
            try
            {
                // Basic logging
                _logger.LogInformation("[GreenAPI Webhook] Received for company {CompanyId}", companyId);

                // Lookup configuration
                var config = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Provider == "GreenAPI" && c.IsActive);

                if (config == null)
                {
                    _logger.LogWarning("[GreenAPI Webhook] No active config for company {CompanyId}", companyId);
                    return NotFound();
                }

                // Validate token from path
                if (string.IsNullOrEmpty(config.WebhookToken) || !string.Equals(config.WebhookToken, token, StringComparison.Ordinal))
                {
                    _logger.LogWarning("[GreenAPI Webhook] Invalid token for company {CompanyId}", companyId);
                    return Unauthorized();
                }

                // Validate header (Authorization by default)
                var headerName = string.IsNullOrWhiteSpace(config.HeaderName) ? "Authorization" : config.HeaderName;
                var expectedTemplate = string.IsNullOrWhiteSpace(config.HeaderValueTemplate) ? "Bearer {secret}" : config.HeaderValueTemplate;
                var providedHeader = Request.Headers[headerName].FirstOrDefault();

                if (!string.IsNullOrEmpty(config.WebhookSecret))
                {
                    var secretPlain = _encryptionService.Decrypt(config.WebhookSecret);
                    var expectedHeaderValue = expectedTemplate.Replace("{secret}", secretPlain);

                    if (!string.Equals(providedHeader, expectedHeaderValue, StringComparison.Ordinal))
                    {
                        _logger.LogWarning("[GreenAPI Webhook] Header auth failed for company {CompanyId}. Header: {HeaderName}", companyId, headerName);
                        return StatusCode(403);
                    }
                }

                // Extract event identifiers for idempotency
                string? eventType = payload?["typeWebhook"]?.ToString();
                string? messageId = payload?["idMessage"]?.ToString()
                                   ?? payload?["instanceData"]?["idMessage"]?.ToString()
                                   ?? payload?["messageData"]?["idMessage"]?.ToString();

                // Ensure event table and persist quickly (best-effort)
                await EnsureWebhookEventsTableAsync();
                if (!string.IsNullOrEmpty(messageId))
                {
                    var inserted = await TryInsertEventAsync(companyId, messageId, eventType, payload?.ToString());
                    if (!inserted)
                    {
                        // Duplicate or error; respond 200 to avoid retries
                        _logger.LogInformation("[GreenAPI Webhook] Duplicate or failed to log event {MessageId}", messageId);
                    }
                }

                // Update last event timestamp
                config.LastWebhookEventAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // TODO: enqueue async processing job here (out of scope)

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GreenAPI Webhook] Unhandled error");
                // Respond OK to avoid repeated retries; the event is logged
                return Ok();
            }
        }

        /// <summary>
        /// Quick GET for health checks of webhook endpoint (no auth)
        /// </summary>
        [HttpGet("{companyId:int}/{token}")]
        [AllowAnonymous]
        public IActionResult Health(int companyId, string token)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "GreenAPI webhook endpoint is live",
                Data = new { companyId, tokenReceived = token.Length > 0 }
            });
        }

        private async Task EnsureWebhookEventsTableAsync()
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS public.""WebhookEvents"" (
  ""Id"" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  ""CompanyId"" integer NOT NULL,
  ""Provider"" varchar(50) NOT NULL,
  ""EventId"" varchar(200),
  ""EventType"" varchar(100),
  ""Payload"" jsonb,
  ""ReceivedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS ""IX_WebhookEvents_Company_Provider"" ON public.""WebhookEvents""(""CompanyId"", ""Provider"");
CREATE UNIQUE INDEX IF NOT EXISTS ""UX_WebhookEvents_Company_EventId"" ON public.""WebhookEvents""(""CompanyId"", ""EventId"") WHERE ""EventId"" IS NOT NULL;";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GreenAPI Webhook] Failed to ensure WebhookEvents table");
            }
        }

        private async Task<bool> TryInsertEventAsync(int companyId, string messageId, string? eventType, string? payload)
        {
            const string insertSql = @"
INSERT INTO public.""WebhookEvents"" (""CompanyId"", ""Provider"", ""EventId"", ""EventType"", ""Payload"")
VALUES ({0}, {1}, {2}, {3}, {4}) ON CONFLICT DO NOTHING;";

            try
            {
                var affected = await _context.Database.ExecuteSqlRawAsync(
                    insertSql,
                    companyId,
                    "GreenAPI",
                    messageId,
                    eventType ?? (object)DBNull.Value!,
                    payload ?? (object)DBNull.Value!
                );
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GreenAPI Webhook] Insert event failed for {MessageId}", messageId);
                return false;
            }
        }
    }
}

