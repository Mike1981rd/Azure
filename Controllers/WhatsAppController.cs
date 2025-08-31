using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsiteBuilderAPI.DTOs.Common;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Controller for WhatsApp Twilio integration
    /// </summary>
    [ApiController]
    [Route("api/whatsapp")]
    [Authorize]
    public class WhatsAppController : ControllerBase
    {
        private readonly ITwilioWhatsAppService _whatsAppService;
        private readonly IWhatsAppServiceFactory? _whatsAppServiceFactory;
        private readonly ILogger<WhatsAppController> _logger;

        public WhatsAppController(
            ITwilioWhatsAppService whatsAppService,
            ILogger<WhatsAppController> logger,
            IWhatsAppServiceFactory? whatsAppServiceFactory = null)
        {
            _whatsAppService = whatsAppService;
            _whatsAppServiceFactory = whatsAppServiceFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the active WhatsApp service (factory-based or legacy)
        /// </summary>
        private IWhatsAppService GetActiveWhatsAppService()
        {
            // Use factory if available, otherwise fallback to legacy service
            return _whatsAppServiceFactory?.GetService() ?? _whatsAppService;
        }

        #region Configuration Endpoints

        /// <summary>
        /// Get WhatsApp configuration for the current company
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var companyId = GetCompanyId();
                var config = await _whatsAppService.GetConfigAsync(companyId);

                if (config == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No WhatsApp configuration found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<WhatsAppConfigDto>
                {
                    Success = true,
                    Message = "Configuration retrieved successfully",
                    Data = config
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving WhatsApp configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving configuration"
                });
            }
        }

        /// <summary>
        /// Create WhatsApp configuration
        /// </summary>
        [HttpPost("config")]
        public async Task<IActionResult> CreateConfig([FromBody] CreateWhatsAppConfigDto dto)
        {
            try
            {
                _logger.LogInformation("CreateConfig called for company {CompanyId} with model state: {ModelState}", GetCompanyId(), ModelState.IsValid);

                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", modelErrors));
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = modelErrors
                    });
                }

                // Custom validation for multi-provider support
                if (!dto.IsValid(out var validationErrors))
                {
                    _logger.LogWarning("Custom validation failed: {Errors}", string.Join(", ", validationErrors));
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Configuration validation failed",
                        Data = validationErrors
                    });
                }

                var companyId = GetCompanyId();
                _logger.LogInformation("Creating WhatsApp config for company {CompanyId}", companyId);
                
                var config = await _whatsAppService.CreateConfigAsync(companyId, dto);

                _logger.LogInformation("WhatsApp configuration created successfully for company {CompanyId}", companyId);
                return Ok(new ApiResponse<WhatsAppConfigDto>
                {
                    Success = true,
                    Message = "WhatsApp configuration created successfully",
                    Data = config
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WhatsApp configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating configuration"
                });
            }
        }

        /// <summary>
        /// Update WhatsApp configuration
        /// </summary>
        [HttpPut("config")]
        public async Task<IActionResult> UpdateConfig([FromBody] UpdateWhatsAppConfigDto dto)
        {
            try
            {
                // Enhanced logging for debugging 401 issues
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                var hasAuthHeader = !string.IsNullOrEmpty(authHeader);
                var isBearer = authHeader?.StartsWith("Bearer ") ?? false;
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                
                _logger.LogInformation("UpdateConfig called - AuthHeader: {HasHeader}, IsBearer: {IsBearer}, IsAuthenticated: {IsAuth}", 
                    hasAuthHeader, isBearer, isAuthenticated);
                
                if (!isAuthenticated)
                {
                    _logger.LogWarning("UpdateConfig: User not authenticated. AuthHeader exists: {HasHeader}, Format correct: {IsBearer}", 
                        hasAuthHeader, isBearer);
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Authentication required. Please ensure you are logged in."
                    });
                }

                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", modelErrors));
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = modelErrors
                    });
                }

                var companyId = GetCompanyId();
                _logger.LogInformation("Updating WhatsApp config for company {CompanyId}", companyId);
                
                var config = await _whatsAppService.UpdateConfigAsync(companyId, dto);

                _logger.LogInformation("WhatsApp configuration updated successfully for company {CompanyId}", companyId);
                return Ok(new ApiResponse<WhatsAppConfigDto>
                {
                    Success = true,
                    Message = "WhatsApp configuration updated successfully",
                    Data = config
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access in UpdateConfig: {Error}", ex.Message);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized: " + ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WhatsApp configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating configuration"
                });
            }
        }

        /// <summary>
        /// Delete WhatsApp configuration
        /// </summary>
        [HttpDelete("config")]
        public async Task<IActionResult> DeleteConfig()
        {
            try
            {
                var companyId = GetCompanyId();
                var deleted = await _whatsAppService.DeleteConfigAsync(companyId);

                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Configuration not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "WhatsApp configuration deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WhatsApp configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting configuration"
                });
            }
        }

        /// <summary>
        /// Test WhatsApp configuration
        /// </summary>
        [HttpPost("config/test")]
        public async Task<IActionResult> TestConfig([FromBody] TestWhatsAppConfigDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var companyId = GetCompanyId();
                _logger.LogInformation("Testing WhatsApp config for company {CompanyId} with phone {Phone}", 
                    companyId, dto.TestPhoneNumber);
                
                var result = await _whatsAppService.TestConfigAsync(companyId, dto);

                return Ok(new ApiResponse<WhatsAppConfigTestResultDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("WhatsApp config not found for company {CompanyId}", GetCompanyId());
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing WhatsApp configuration for company {CompanyId}", GetCompanyId());
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error testing configuration: {ex.Message}"
                });
            }
        }

        #endregion

        #region Conversation Endpoints

        /// <summary>
        /// Get all conversations
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var companyId = GetCompanyId();
                var filter = new WhatsAppConversationFilterDto
                {
                    Status = status,
                    SearchTerm = search,
                    Page = page,
                    PageSize = pageSize
                };

                var conversations = await _whatsAppService.GetConversationsAsync(companyId, filter);

                return Ok(new ApiResponse<WhatsAppConversationListDto>
                {
                    Success = true,
                    Message = "Conversations retrieved successfully",
                    Data = conversations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving conversations"
                });
            }
        }

        /// <summary>
        /// Get conversation detail by ID
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversationDetail(Guid conversationId)
        {
            try
            {
                // Validate GUID parameter
                if (conversationId == Guid.Empty)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid conversationId format. Expected a valid GUID."
                    });
                }

                var companyId = GetCompanyId();
                var conversation = await _whatsAppService.GetConversationDetailAsync(companyId, conversationId);

                if (conversation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                return Ok(new ApiResponse<WhatsAppConversationDetailDto>
                {
                    Success = true,
                    Message = "Conversation retrieved successfully",
                    Data = conversation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving conversation"
                });
            }
        }

        /// <summary>
        /// Update conversation
        /// </summary>
        [HttpPut("conversations/{conversationId}")]
        public async Task<IActionResult> UpdateConversation(Guid conversationId, [FromBody] UpdateWhatsAppConversationDto dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var conversation = await _whatsAppService.UpdateConversationAsync(companyId, conversationId, dto);

                return Ok(new ApiResponse<WhatsAppConversationDto>
                {
                    Success = true,
                    Message = "Conversation updated successfully",
                    Data = conversation
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating conversation"
                });
            }
        }

        /// <summary>
        /// Close conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/close")]
        public async Task<IActionResult> CloseConversation(Guid conversationId)
        {
            try
            {
                var companyId = GetCompanyId();
                var success = await _whatsAppService.CloseConversationAsync(companyId, conversationId);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Conversation closed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing conversation {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error closing conversation"
                });
            }
        }

        /// <summary>
        /// Archive conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/archive")]
        public async Task<IActionResult> ArchiveConversation(Guid conversationId)
        {
            try
            {
                var companyId = GetCompanyId();
                var success = await _whatsAppService.ArchiveConversationAsync(companyId, conversationId);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Conversation archived successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error archiving conversation"
                });
            }
        }

        /// <summary>
        /// Poll for conversation updates since a given time
        /// </summary>
        [HttpGet("conversations/updates")]
        public async Task<IActionResult> PollConversationUpdates([FromQuery] DateTime? since)
        {
            try
            {
                var companyId = GetCompanyId();
                var sinceTime = since ?? DateTime.UtcNow.AddMinutes(-5);

                // Get conversations updated since the given time
                var filter = new WhatsAppConversationFilterDto
                {
                    Page = 1,
                    PageSize = 50
                };

                var conversations = await _whatsAppService.GetConversationsAsync(companyId, filter);
                
                // Filter for recently updated conversations
                var updatedConversations = conversations.Conversations
                    .Where(c => c.LastMessageAt > sinceTime || c.StartedAt > sinceTime)
                    .ToList();

                return Ok(new ApiResponse<List<WhatsAppConversationDto>>
                {
                    Success = true,
                    Message = "Recent conversation updates retrieved",
                    Data = updatedConversations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling conversation updates");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error polling conversation updates"
                });
            }
        }

        /// <summary>
        /// Poll for conversation updates since a given time (with company ID in path)
        /// </summary>
        [HttpGet("conversations/{companyId}/updates")]
        public async Task<IActionResult> PollConversationUpdatesWithCompanyId(int companyId, [FromQuery] DateTime? since)
        {
            try
            {
                // Validate company access - in production you'd check if user has access to this company
                var userCompanyId = GetCompanyId();
                if (userCompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                var sinceTime = since ?? DateTime.UtcNow.AddMinutes(-5);

                // Get conversations updated since the given time
                var filter = new WhatsAppConversationFilterDto
                {
                    Page = 1,
                    PageSize = 50
                };

                var conversations = await _whatsAppService.GetConversationsAsync(companyId, filter);
                
                // Filter for recently updated conversations
                var updatedConversations = conversations.Conversations
                    .Where(c => c.LastMessageAt > sinceTime || c.StartedAt > sinceTime)
                    .ToList();

                return Ok(new ApiResponse<List<WhatsAppConversationDto>>
                {
                    Success = true,
                    Message = "Recent conversation updates retrieved",
                    Data = updatedConversations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling conversation updates for company {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error polling conversation updates"
                });
            }
        }

        #endregion

        #region Message Endpoints

        /// <summary>
        /// Poll for new messages since a given time
        /// </summary>
        [HttpGet("messages/new")]
        public async Task<IActionResult> PollNewMessages([FromQuery] DateTime? since)
        {
            try
            {
                var companyId = GetCompanyId();
                var sinceTime = since ?? DateTime.UtcNow.AddMinutes(-5);

                // This is a simplified implementation. In production, you'd want to
                // query messages directly from the database for better performance
                var newMessages = new List<WhatsAppMessageDto>();

                return Ok(new ApiResponse<List<WhatsAppMessageDto>>
                {
                    Success = true,
                    Message = "Recent messages retrieved",
                    Data = newMessages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling new messages");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error polling new messages"
                });
            }
        }

        /// <summary>
        /// Poll for new messages since a given time (with company ID in path)
        /// </summary>
        [HttpGet("messages/new/{companyId}")]
        public async Task<IActionResult> PollNewMessagesWithCompanyId(int companyId, [FromQuery] DateTime? since)
        {
            try
            {
                // Validate company access - in production you'd check if user has access to this company
                var userCompanyId = GetCompanyId();
                if (userCompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                var sinceTime = since ?? DateTime.UtcNow.AddMinutes(-5);

                // This is a simplified implementation. In production, you'd want to
                // query messages directly from the database for better performance
                var newMessages = new List<WhatsAppMessageDto>();

                return Ok(new ApiResponse<List<WhatsAppMessageDto>>
                {
                    Success = true,
                    Message = "Recent messages retrieved",
                    Data = newMessages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling new messages");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error polling new messages"
                });
            }
        }

        /// <summary>
        /// Send WhatsApp message
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendWhatsAppMessageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var companyId = GetCompanyId();
                var message = await _whatsAppService.SendMessageAsync(companyId, dto);

                return Ok(new ApiResponse<WhatsAppMessageDto>
                {
                    Success = true,
                    Message = "Message sent successfully",
                    Data = message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message to {To}", dto.To);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error sending message"
                });
            }
        }

        /// <summary>
        /// Send bulk WhatsApp messages
        /// </summary>
        [HttpPost("send/bulk")]
        public async Task<IActionResult> SendBulkMessage([FromBody] BulkSendWhatsAppMessageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var companyId = GetCompanyId();
                var messages = await _whatsAppService.SendBulkMessageAsync(companyId, dto);

                return Ok(new ApiResponse<List<WhatsAppMessageDto>>
                {
                    Success = true,
                    Message = $"Bulk messages sent successfully. {messages.Count} out of {dto.To.Count} messages sent.",
                    Data = messages
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk WhatsApp messages");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error sending bulk messages"
                });
            }
        }

        /// <summary>
        /// Send template message
        /// </summary>
        [HttpPost("send/template")]
        public async Task<IActionResult> SendTemplateMessage([FromBody] SendTemplateMessageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var companyId = GetCompanyId();
                var message = await _whatsAppService.SendTemplateMessageAsync(companyId, dto.To, dto.TemplateId, dto.Parameters);

                return Ok(new ApiResponse<WhatsAppMessageDto>
                {
                    Success = true,
                    Message = "Template message sent successfully",
                    Data = message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template message to {To}", dto.To);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error sending template message"
                });
            }
        }

        /// <summary>
        /// Get messages for a conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var companyId = GetCompanyId();
                var messages = await _whatsAppService.GetMessagesAsync(companyId, conversationId, page, pageSize);

                return Ok(new ApiResponse<List<WhatsAppMessageDto>>
                {
                    Success = true,
                    Message = "Messages retrieved successfully",
                    Data = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving messages"
                });
            }
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        [HttpPut("messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(Guid messageId)
        {
            try
            {
                var companyId = GetCompanyId();
                var success = await _whatsAppService.MarkMessageAsReadAsync(companyId, messageId);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Message not found or already read"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Message marked as read"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read {MessageId}", messageId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error marking message as read"
                });
            }
        }

        /// <summary>
        /// Mark conversation as read
        /// </summary>
        [HttpPut("conversations/{conversationId}/read")]
        public async Task<IActionResult> MarkConversationAsRead(Guid conversationId)
        {
            try
            {
                var companyId = GetCompanyId();
                var success = await _whatsAppService.MarkConversationAsReadAsync(companyId, conversationId);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Conversation marked as read"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation as read {ConversationId}", conversationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error marking conversation as read"
                });
            }
        }

        #endregion

        #region Webhook Endpoints

        /* COMMENTED OUT - Duplicate route with PublicWhatsAppWebhookController
        /// <summary>
        /// Webhook endpoint for receiving incoming messages from Twilio
        /// NOTE: This endpoint is now handled by PublicWhatsAppWebhookController to avoid route conflicts
        /// </summary>
        [HttpPost("webhook/incoming")]
        [AllowAnonymous]
        public async Task<IActionResult> WebhookIncoming([FromForm] TwilioWebhookDto webhookData)
        {
            try
            {
                // Validate webhook signature
                var signature = Request.Headers["X-Twilio-Signature"].FirstOrDefault();
                if (!string.IsNullOrEmpty(signature))
                {
                    var body = await GetRawBodyAsync();
                    var isValid = await _whatsAppService.ValidateWebhookSignatureAsync(signature, body);
                    
                    if (!isValid)
                    {
                        _logger.LogWarning("Invalid webhook signature received");
                        return Unauthorized();
                    }
                }

                var success = await _whatsAppService.ProcessIncomingMessageAsync(webhookData);

                if (success)
                {
                    return Ok("Message processed");
                }

                return BadRequest("Failed to process message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming webhook");
                return StatusCode(500, "Error processing webhook");
            }
        }
        */

        /* COMMENTED OUT - Duplicate route with PublicWhatsAppWebhookController
        /// <summary>
        /// Webhook endpoint for message status updates from Twilio
        /// NOTE: This endpoint is now handled by PublicWhatsAppWebhookController to avoid route conflicts
        /// </summary>
        [HttpPost("webhook/status")]
        [AllowAnonymous]
        public async Task<IActionResult> WebhookStatus([FromForm] TwilioStatusWebhookDto statusData)
        {
            try
            {
                var success = await _whatsAppService.ProcessMessageStatusAsync(statusData);

                if (success)
                {
                    return Ok("Status processed");
                }

                return BadRequest("Failed to process status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing status webhook");
                return StatusCode(500, "Error processing webhook");
            }
        }
        */

        #endregion

        #region Provider Management Endpoints

        /// <summary>
        /// Get information about available WhatsApp providers
        /// </summary>
        [HttpGet("providers")]
        public IActionResult GetAvailableProviders()
        {
            try
            {
                if (_whatsAppServiceFactory == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Legacy Twilio-only mode",
                        Data = new { Providers = new[] { "Twilio" }, ActiveProvider = "Twilio" }
                    });
                }

                var providers = _whatsAppServiceFactory.GetAvailableProviders();
                var activeProvider = _whatsAppServiceFactory.GetActiveProvider();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "WhatsApp providers information",
                    Data = new
                    {
                        Providers = providers,
                        ActiveProvider = activeProvider
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WhatsApp providers information");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving providers information"
                });
            }
        }

        /// <summary>
        /// Test a specific provider configuration
        /// </summary>
        [HttpPost("test-provider/{providerName}")]
        public async Task<IActionResult> TestSpecificProvider(string providerName, [FromBody] TestWhatsAppConfigDto dto)
        {
            try
            {
                if (_whatsAppServiceFactory == null)
                {
                    if (providerName.ToUpperInvariant() == "TWILIO")
                    {
                        var testCompanyId = GetCompanyId();
                        var testResult = await _whatsAppService.TestConfigAsync(testCompanyId, dto);
                        return Ok(new ApiResponse<WhatsAppConfigTestResultDto>
                        {
                            Success = testResult.Success,
                            Message = testResult.Message,
                            Data = testResult
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Only Twilio provider is available in legacy mode"
                        });
                    }
                }

                var service = _whatsAppServiceFactory.GetService(providerName);
                var companyId = GetCompanyId();
                var result = await service.TestConfigAsync(companyId, dto);

                return Ok(new ApiResponse<WhatsAppConfigTestResultDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid provider requested: {ProviderName}", providerName);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing {ProviderName} configuration", providerName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error testing {providerName} configuration: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get provider-specific status information
        /// </summary>
        [HttpGet("provider-status")]
        public IActionResult GetProviderStatus()
        {
            try
            {
                var companyId = GetCompanyId();

                if (_whatsAppServiceFactory == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Data = new
                        {
                            Mode = "Legacy",
                            Provider = "Twilio",
                            IsConfigured = _whatsAppService.IsWithinBusinessHoursAsync(companyId).GetAwaiter().GetResult()
                        }
                    });
                }

                var activeProvider = _whatsAppServiceFactory.GetActiveProvider();
                var service = _whatsAppServiceFactory.GetService();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Mode = "Factory",
                        Provider = activeProvider,
                        ProviderName = service.ProviderName,
                        IsConfigured = service.IsConfigured(companyId)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving provider status"
                });
            }
        }

        #endregion

        #region Debug/Testing Endpoints

        /// <summary>
        /// Debug endpoint to analyze complete authentication state
        /// </summary>
        [HttpGet("debug/auth")]
        [AllowAnonymous]
        public IActionResult DebugAuth()
        {
            try
            {
                var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                
                // Try to extract company ID safely
                int? companyId = null;
                try
                {
                    companyId = GetCompanyId();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not extract company ID: {Error}", ex.Message);
                }
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Debug info retrieved",
                    Data = new
                    {
                        headers,
                        authHeader,
                        authHeaderExists = !string.IsNullOrEmpty(authHeader),
                        authHeaderFormat = authHeader?.StartsWith("Bearer ") ?? false,
                        claims,
                        isAuthenticated,
                        identity = User.Identity?.Name,
                        authenticationType = User.Identity?.AuthenticationType,
                        companyId,
                        timestamp = DateTime.UtcNow,
                        requestPath = Request.Path.ToString(),
                        userAgent = Request.Headers["User-Agent"].ToString(),
                        origin = Request.Headers["Origin"].ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug auth endpoint");
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new
                    {
                        error = ex.ToString(),
                        timestamp = DateTime.UtcNow
                    }
                });
            }
        }

        // Method removed to fix route ambiguity - use WhatsAppTestController.GetCurrentProvider instead

        /// <summary>
        /// Test specific provider configuration
        /// </summary>
        [HttpPost("test/test-config/{provider}")]
        public async Task<IActionResult> TestProviderConfig(string provider, [FromBody] TestWhatsAppConfigDto dto)
        {
            try
            {
                _logger.LogInformation("Testing configuration for provider: {Provider}", provider);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var companyId = GetCompanyId();
                
                // Use factory if available
                if (_whatsAppServiceFactory != null)
                {
                    var service = _whatsAppServiceFactory.GetService(provider);
                    var result = await service.TestConfigAsync(companyId, dto);

                    return Ok(new ApiResponse<WhatsAppConfigTestResultDto>
                    {
                        Success = result.Success,
                        Message = result.Message,
                        Data = result
                    });
                }
                else
                {
                    // Legacy mode - only support Twilio
                    if (provider.ToUpperInvariant() == "TWILIO")
                    {
                        var result = await _whatsAppService.TestConfigAsync(companyId, dto);
                        return Ok(new ApiResponse<WhatsAppConfigTestResultDto>
                        {
                            Success = result.Success,
                            Message = result.Message,
                            Data = result
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Only Twilio provider is available in legacy mode"
                        });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid provider requested: {Provider}", provider);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing {Provider} configuration", provider);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error testing {provider} configuration: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Test authentication and authorization
        /// </summary>
        [HttpGet("test/auth")]
        public IActionResult TestAuth()
        {
            try
            {
                var companyId = GetCompanyId();
                var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Authentication successful",
                    Data = new
                    {
                        CompanyId = companyId,
                        UserId = User.Identity?.Name,
                        IsAuthenticated = User.Identity?.IsAuthenticated,
                        AuthenticationType = User.Identity?.AuthenticationType,
                        Claims = claims
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auth test");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Test endpoint to validate GUID parameter binding
        /// </summary>
        [HttpGet("test/guid-binding/{testGuid}")]
        public IActionResult TestGuidBinding(Guid testGuid)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "GUID binding successful",
                Data = new
                {
                    ReceivedGuid = testGuid,
                    IsEmpty = testGuid == Guid.Empty,
                    StringRepresentation = testGuid.ToString()
                }
            });
        }

        /// <summary>
        /// Create a test conversation with valid GUID
        /// </summary>
        [HttpPost("test/create-test-conversation")]
        public async Task<IActionResult> CreateTestConversation()
        {
            try
            {
                var companyId = GetCompanyId();
                
                // Create a test conversation
                var testConversation = await _whatsAppService.GetOrCreateConversationAsync(
                    companyId, 
                    "+1234567890", // Test phone number
                    "+14155238886"  // Test business phone
                );

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test conversation created successfully",
                    Data = new
                    {
                        ConversationId = testConversation?.Id,
                        TestUrl = $"/api/whatsapp/conversations/{testConversation?.Id}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test conversation");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error creating test conversation"
                });
            }
        }

        /// <summary>
        /// Alternative config update without strict auth (TEMPORARY FOR DEBUGGING)
        /// </summary>
        [HttpPost("config/debug-update")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugUpdateConfig(
            [FromBody] UpdateWhatsAppConfigDto dto,
            [FromQuery] int? companyId = null)
        {
            try
            {
                _logger.LogInformation("DebugUpdateConfig called - CompanyId from query: {CompanyId}", companyId);
                
                // Use provided companyId or try to extract from token, default to 1
                var targetCompanyId = companyId ?? 1;
                
                if (User.Identity?.IsAuthenticated == true)
                {
                    try
                    {
                        targetCompanyId = GetCompanyId();
                        _logger.LogInformation("Using company ID from token: {CompanyId}", targetCompanyId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not extract company ID from token: {Error}. Using provided: {CompanyId}", 
                            ex.Message, targetCompanyId);
                    }
                }
                else
                {
                    _logger.LogWarning("User not authenticated, using provided company ID: {CompanyId}", targetCompanyId);
                }

                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = modelErrors
                    });
                }
                
                var config = await _whatsAppService.UpdateConfigAsync(targetCompanyId, dto);

                return Ok(new ApiResponse<WhatsAppConfigDto>
                {
                    Success = true,
                    Message = "[DEBUG] WhatsApp configuration updated successfully",
                    Data = config
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug update config");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating configuration: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Test JWT token validation specifically
        /// </summary>
        [HttpPost("debug/validate-token")]
        [AllowAnonymous]
        public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Token validation requested");
                
                if (string.IsNullOrEmpty(request?.Token))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token is required"
                    });
                }

                // Try to manually validate the JWT token
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                
                if (!tokenHandler.CanReadToken(request.Token))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token format is invalid",
                        Data = new { TokenLength = request.Token.Length }
                    });
                }

                var token = tokenHandler.ReadJwtToken(request.Token);
                var claims = token.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var companyIdClaim = claims.FirstOrDefault(c => c.Type.ToLower().Contains("companyid") || c.Type.ToLower().Contains("company_id"));
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token parsed successfully",
                    Data = new
                    {
                        IssuedAt = token.IssuedAt,
                        ExpiresAt = token.ValidTo,
                        IsExpired = token.ValidTo < DateTime.UtcNow,
                        Issuer = token.Issuer,
                        Audience = token.Audiences?.FirstOrDefault(),
                        Claims = claims,
                        CompanyIdClaim = companyIdClaim,
                        TokenLength = request.Token.Length
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token validation failed: " + ex.Message,
                    Data = new { Error = ex.ToString() }
                });
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Get company ID from current user claims
        /// </summary>
        private int GetCompanyId()
        {
            try
            {
                // Log all claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}:{c.Value}").ToArray();
                _logger.LogDebug("User claims: {Claims}", string.Join(", ", claims));
                
                // Try both claim names for backward compatibility
                // CRITICAL: Use lowercase "companyId" per Guardado.md
                var companyIdClaim = User.FindFirst("companyId")?.Value ?? 
                                   User.FindFirst("CompanyId")?.Value ?? 
                                   User.FindFirst("company_id")?.Value;
                
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    // Fallback for single-tenant just like CustomersController
                    _logger.LogWarning("Company ID claim not found or invalid. Using fallback companyId = 1. Available claims: {Claims}", string.Join(", ", claims));
                    companyId = 1; // Fallback for single-tenant
                }
                else
                {
                    _logger.LogDebug("Successfully extracted company ID: {CompanyId}", companyId);
                }

                return companyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting company ID from user claims, using fallback");
                return 1; // Fallback for single-tenant
            }
        }

        /// <summary>
        /// Get raw request body for webhook signature validation
        /// </summary>
        private async Task<string> GetRawBodyAsync()
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body);
            return await reader.ReadToEndAsync();
        }

        #endregion
    }

    /// <summary>
    /// DTO for sending template messages
    /// </summary>
    public class SendTemplateMessageDto
    {
        public string To { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public Dictionary<string, string>? Parameters { get; set; }
    }

    /// <summary>
    /// DTO for token validation request
    /// </summary>
    public class TokenValidationRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}