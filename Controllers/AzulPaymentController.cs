using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebsiteBuilderAPI.DTOs.Azul;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/payments/azul")]
    public class AzulPaymentController : ControllerBase
    {
        private readonly IAzulPaymentService _azulService;
        private readonly ILogger<AzulPaymentController> _logger;

        public AzulPaymentController(
            IAzulPaymentService azulService,
            ILogger<AzulPaymentController> logger)
        {
            _azulService = azulService;
            _logger = logger;
        }

        [HttpPost("charge")]
        public async Task<ActionResult> ProcessAzulPayment([FromBody] AzulPaymentRequestDto request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Process with Azul (certificates in Azure)
                // NOTE: AzulPaymentRequestDto doesn't include CompanyId in current DTO version.
                // Using default companyId = 1 or tenant resolution to be implemented.
                var companyId = 1;
                var azulResponse = await _azulService.ProcessPaymentAsync(request, companyId);
                
                // Log payment result
                int.TryParse(request.OrderNumber, out var orderId);
                var approved = string.Equals(azulResponse.ResponseCode, "00", StringComparison.OrdinalIgnoreCase);
                _logger.LogInformation("Azul payment processed for order {OrderId} with status: {Status}", 
                    orderId, approved ? "completed" : "failed");


                return Ok(new
                {
                    success = approved,
                    transactionId = azulResponse.RRN,
                    message = azulResponse.ResponseMessage,
                    responseCode = azulResponse.ResponseCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azul payment failed for order {OrderNumber}", request.OrderNumber);
                

                return StatusCode(500, new { error = "Payment processing failed", message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> HandleAzulWebhook()
        {
            try
            {
                // Read body raw for signature verification
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                // Verify signature from Azul (not implemented in current service interface)
                var signature = Request.Headers["X-Azul-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Azul webhook received without signature header");
                }

                // Parse webhook payload
                var webhook = JsonSerializer.Deserialize<AzulWebhookDto>(body);
                if (webhook == null)
                {
                    _logger.LogWarning("Invalid webhook payload");
                    return BadRequest();
                }
                
                // Log the webhook event
                _logger.LogInformation("Azul webhook received for order {OrderId} with status {Status}", 
                    webhook.OrderId, webhook.Status);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Azul webhook");
                return StatusCode(500);
            }
        }

        [HttpGet("test-connection")]
        public ActionResult TestConnection()
        {
            try
            {
                // Verify Azul configuration
                var ok = _azulService.ValidateCredentialsAsync(1).GetAwaiter().GetResult();
                return Ok(new { ok, message = ok ? "Azul connection test successful" : "Azul credentials invalid" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Configuration error", message = ex.Message });
            }
        }
    }

    public class AzulWebhookDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AzulOrderId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
