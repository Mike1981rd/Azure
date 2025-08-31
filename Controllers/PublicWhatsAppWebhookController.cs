using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Public webhook controller for Twilio WhatsApp integration
    /// This controller is separate from the main WhatsApp controller to handle webhooks without authentication
    /// </summary>
    [ApiController]
    [Route("api/whatsapp/webhook")]
    [AllowAnonymous] // Explicitly allow anonymous access for all endpoints
    public class PublicWhatsAppWebhookController : ControllerBase
    {
        private readonly ITwilioWhatsAppService _whatsAppService;
        private readonly ILogger<PublicWhatsAppWebhookController> _logger;

        public PublicWhatsAppWebhookController(
            ITwilioWhatsAppService whatsAppService,
            ILogger<PublicWhatsAppWebhookController> logger)
        {
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook endpoint for receiving incoming messages from Twilio
        /// This endpoint must be publicly accessible without authentication
        /// </summary>
        [HttpPost("incoming")]
        [AllowAnonymous]
        public async Task<IActionResult> IncomingMessage([FromForm] TwilioWebhookDto webhookData)
        {
            try
            {
                _logger.LogInformation("Received incoming WhatsApp webhook from {From} to {To}", 
                    webhookData?.From, webhookData?.To);

                // Log the webhook data for debugging
                if (webhookData != null)
                {
                    _logger.LogDebug("Webhook data: MessageSid={MessageSid}, Body={Body}, NumMedia={NumMedia}", 
                        webhookData.MessageSid, webhookData.Body, webhookData.NumMedia);
                }

                // Validate webhook signature if configured
                var signature = Request.Headers["X-Twilio-Signature"].FirstOrDefault();
                if (!string.IsNullOrEmpty(signature))
                {
                    _logger.LogDebug("Validating webhook signature");
                    
                    // Get the full URL that Twilio used
                    var fullUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
                    
                    // For production, ensure we use the correct public URL
                    if (Request.Host.Value.Contains("localhost") == false)
                    {
                        fullUrl = $"https://api.test1hotelwebsite.online{Request.Path}";
                    }
                    
                    _logger.LogDebug("Webhook URL for validation: {Url}", fullUrl);
                    
                    // Get form data as dictionary for validation
                    var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                    
                    var isValid = await _whatsAppService.ValidateWebhookSignatureAsync(signature, fullUrl, formData);
                    
                    if (!isValid)
                    {
                        _logger.LogWarning("Invalid webhook signature received for URL: {Url}", fullUrl);
                        // In production, you might want to return Unauthorized
                        // For now, we'll just log the warning and continue
                    }
                }
                else
                {
                    _logger.LogDebug("No webhook signature provided");
                }

                // Process the incoming message
                var success = await _whatsAppService.ProcessIncomingMessageAsync(webhookData);

                if (success)
                {
                    _logger.LogInformation("Successfully processed incoming message from {From}", webhookData?.From);
                    // Return TwiML response (empty response is fine)
                    return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "text/xml");
                }

                _logger.LogWarning("Failed to process incoming message from {From}", webhookData?.From);
                return BadRequest("Failed to process message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming WhatsApp webhook");
                // Return OK to prevent Twilio from retrying
                return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "text/xml");
            }
        }

        /// <summary>
        /// Webhook endpoint for message status updates from Twilio
        /// This endpoint must be publicly accessible without authentication
        /// </summary>
        [HttpPost("status")]
        [AllowAnonymous]
        public async Task<IActionResult> StatusUpdate([FromForm] TwilioStatusWebhookDto statusData)
        {
            try
            {
                _logger.LogInformation("Received status webhook for MessageSid: {MessageSid}, Status: {Status}", 
                    statusData?.MessageSid, statusData?.MessageStatus);

                // Process the status update
                var success = await _whatsAppService.ProcessMessageStatusAsync(statusData);

                if (success)
                {
                    _logger.LogInformation("Successfully processed status update for {MessageSid}", statusData?.MessageSid);
                    return Ok();
                }

                _logger.LogWarning("Failed to process status update for {MessageSid}", statusData?.MessageSid);
                return Ok(); // Still return OK to prevent retries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing status webhook for MessageSid: {MessageSid}", statusData?.MessageSid);
                // Return OK to prevent Twilio from retrying
                return Ok();
            }
        }

        /// <summary>
        /// Test endpoint to verify webhook is accessible
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            _logger.LogInformation("WhatsApp webhook test endpoint accessed");
            return Ok(new 
            { 
                status = "OK",
                message = "WhatsApp webhook endpoint is accessible",
                timestamp = DateTime.UtcNow,
                path = Request.Path.Value
            });
        }

        /// <summary>
        /// Health check endpoint for monitoring
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "Healthy",
                service = "WhatsApp Webhook",
                timestamp = DateTime.UtcNow
            });
        }
    }
}