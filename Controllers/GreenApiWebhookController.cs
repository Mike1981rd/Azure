using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebsiteBuilderAPI.DTOs.Common;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Simple webhook controller for Green API WhatsApp
    /// </summary>
    [ApiController]
    [Route("api/whatsapp")]
    public class GreenApiWebhookController : ControllerBase
    {
        private readonly ILogger<GreenApiWebhookController> _logger;

        public GreenApiWebhookController(ILogger<GreenApiWebhookController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Webhook endpoint for receiving messages from Green API
        /// </summary>
        [HttpPost("green-api/webhook")]
        [AllowAnonymous]
        public IActionResult GreenApiWebhook([FromBody] JObject payload)
        {
            try
            {
                _logger.LogInformation("[GREEN API WEBHOOK] Received at {Time}", DateTime.UtcNow);
                
                // Log the raw payload for debugging
                var jsonString = payload?.ToString();
                if (!string.IsNullOrEmpty(jsonString))
                {
                    _logger.LogInformation("[GREEN API WEBHOOK] Payload: {Payload}", jsonString);
                }

                // Extract webhook type
                var typeWebhook = payload?["typeWebhook"]?.ToString();
                _logger.LogInformation("[GREEN API WEBHOOK] Type: {Type}", typeWebhook ?? "unknown");

                // Process incoming message
                if (typeWebhook == "incomingMessageReceived")
                {
                    try
                    {
                        var from = payload?["senderData"]?["chatId"]?.ToString();
                        var senderName = payload?["senderData"]?["senderName"]?.ToString() ?? 
                                       payload?["senderData"]?["pushName"]?.ToString();
                        var textMessage = payload?["messageData"]?["textMessageData"]?["textMessage"]?.ToString();
                        var idMessage = payload?["instanceData"]?["idMessage"]?.ToString();

                        _logger.LogInformation("[GREEN API MESSAGE] From: {From} ({Name}), Message: {Message}, ID: {Id}", 
                            from ?? "unknown", 
                            senderName ?? "unknown", 
                            textMessage ?? "empty", 
                            idMessage ?? "unknown");

                        // TODO: Save message to database
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[GREEN API WEBHOOK] Error processing incoming message");
                    }
                }
                else if (typeWebhook == "outgoingMessageStatus")
                {
                    var idMessage = payload?["idMessage"]?.ToString();
                    var status = payload?["status"]?.ToString();
                    _logger.LogInformation("[GREEN API STATUS] Message {MessageId} status: {Status}", 
                        idMessage ?? "unknown", 
                        status ?? "unknown");
                }
                else if (typeWebhook == "stateInstanceChanged")
                {
                    var stateInstance = payload?["stateInstance"]?.ToString();
                    _logger.LogInformation("[GREEN API STATE] Instance state changed to: {State}", 
                        stateInstance ?? "unknown");
                }

                // Always return 200 to prevent Green API from retrying
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GREEN API WEBHOOK] Unhandled error");
                // Still return 200 to prevent retries
                return Ok();
            }
        }

        /// <summary>
        /// Test endpoint to verify webhook is accessible
        /// </summary>
        [HttpGet("green-api/webhook")]
        [AllowAnonymous]
        public IActionResult TestWebhook()
        {
            _logger.LogInformation("[GREEN API TEST] Webhook test endpoint accessed");
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Green API webhook endpoint is active and ready to receive messages",
                Data = new
                {
                    endpoint = "https://api.test1hotelwebsite.online/api/whatsapp/green-api/webhook",
                    method = "POST",
                    contentType = "application/json",
                    serverTime = DateTime.UtcNow,
                    status = "active"
                }
            });
        }
    }
}