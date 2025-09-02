using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Common;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Controller for WhatsApp Widget integration
    /// Handles messages from website chat widget
    /// </summary>
    [ApiController]
    [Route("api/whatsapp/widget")]
    public class WhatsAppWidgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WhatsAppWidgetController> _logger;

        public WhatsAppWidgetController(
            ApplicationDbContext context,
            ILogger<WhatsAppWidgetController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Receive a message from the website widget
        /// </summary>
        [HttpPost("message")]
        public async Task<IActionResult> ReceiveWidgetMessage([FromBody] WidgetMessageDto dto)
        {
            try
            {
                // Get company ID from subdomain or default
                var companyId = GetCompanyId();

                // Get client IP
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                dto.IpAddress = ipAddress;

                // Find or create conversation
                var conversation = await _context.WhatsAppConversations
                    .FirstOrDefaultAsync(c => c.SessionId == dto.SessionId && c.CompanyId == companyId);

                if (conversation == null)
                {
                    // Create new conversation for widget
                    conversation = new WhatsAppConversation
                    {
                        Id = Guid.NewGuid(),
                        CustomerPhone = dto.CustomerPhone ?? "widget",  // Use simple "widget" for widget messages
                        CustomerName = dto.CustomerName ?? "Website Visitor",
                        CustomerEmail = dto.CustomerEmail,
                        BusinessPhone = "widget", // Special identifier for widget conversations
                        Status = "active",
                        Priority = "normal",
                        CompanyId = companyId,
                        Source = "widget",
                        SessionId = dto.SessionId,
                        UnreadCount = 1,
                        MessageCount = 1,
                        LastMessagePreview = dto.Message.Length > 100 ? dto.Message.Substring(0, 100) + "..." : dto.Message,
                        LastMessageAt = DateTime.UtcNow,
                        LastMessageSender = "customer",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.WhatsAppConversations.Add(conversation);
                }
                else
                {
                    // Update existing conversation
                    conversation.UnreadCount++;
                    conversation.MessageCount++;
                    conversation.LastMessagePreview = dto.Message.Length > 100 ? dto.Message.Substring(0, 100) + "..." : dto.Message;
                    conversation.LastMessageAt = DateTime.UtcNow;
                    conversation.LastMessageSender = "customer";
                    conversation.UpdatedAt = DateTime.UtcNow;
                    
                    // Update customer info if provided
                    if (!string.IsNullOrEmpty(dto.CustomerName))
                        conversation.CustomerName = dto.CustomerName;
                    if (!string.IsNullOrEmpty(dto.CustomerEmail))
                        conversation.CustomerEmail = dto.CustomerEmail;
                }

                // Idempotency for client-sent messages: if ClientMessageId provided, avoid duplicates
                if (!string.IsNullOrWhiteSpace(dto.ClientMessageId))
                {
                    var exists = await _context.WhatsAppMessages.AsNoTracking()
                        .Where(m => m.CompanyId == companyId && m.ConversationId == conversation.Id)
                        .Where(m => m.Direction == "inbound" && m.Source == "widget")
                        .Where(m => m.TwilioSid == dto.ClientMessageId)
                        .FirstOrDefaultAsync();
                    if (exists != null)
                    {
                        return Ok(new ApiResponse<object>
                        {
                            Success = true,
                            Message = "Duplicate suppressed",
                            Data = new
                            {
                                conversationId = conversation.Id,
                                messageId = exists.Id,
                                sessionId = dto.SessionId
                            }
                        });
                    }
                }

                // Create the message
                var message = new WhatsAppMessage
                {
                    Id = Guid.NewGuid(),
                    TwilioSid = !string.IsNullOrWhiteSpace(dto.ClientMessageId)
                        ? dto.ClientMessageId!.Length > 100 ? dto.ClientMessageId!.Substring(0, 100) : dto.ClientMessageId!
                        : $"WDG{Guid.NewGuid().ToString("N").Substring(0, 12)}",  // Max 15 chars
                    From = dto.CustomerPhone ?? "widget",  // Use simple "widget" for widget messages
                    To = "business",
                    Body = dto.Message,
                    MessageType = "text",
                    Direction = "inbound",
                    Status = "received",
                    ConversationId = conversation.Id,
                    CompanyId = companyId,
                    Source = "widget",
                    SessionId = dto.SessionId,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        pageUrl = dto.PageUrl,
                        userAgent = dto.UserAgent,
                        ipAddress = dto.IpAddress,
                        customerName = dto.CustomerName,
                        customerEmail = dto.CustomerEmail
                    })
                };

                _context.WhatsAppMessages.Add(message);
                await _context.SaveChangesAsync();

                // Invalidate/refresh message cache for this conversation if using Green API service
                try { Services.GreenApiWhatsAppService.InvalidateMessagesCache(companyId, conversation.Id); } catch { }

                _logger.LogInformation("Widget message received from session {SessionId}", dto.SessionId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Message received successfully",
                    Data = new
                    {
                        conversationId = conversation.Id,
                        messageId = message.Id,
                        sessionId = dto.SessionId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving widget message");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error processing message"
                });
            }
        }

        /// <summary>
        /// Send a response back to the widget
        /// </summary>
        [HttpPost("conversation/{conversationId}/respond")]
        public async Task<IActionResult> SendResponseToWidget(Guid conversationId, [FromBody] WidgetResponseDto dto)
        {
            try
            {
                // Validate input to avoid null reference issues
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Message))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request: message is required"
                    });
                }

                var companyId = GetCompanyId();

                // Find conversation
                var conversation = await _context.WhatsAppConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                // Idempotency: prefer client-provided id; fallback to same-body window
                WhatsAppMessage? existing = null;
                if (!string.IsNullOrWhiteSpace(dto.ClientMessageId))
                {
                    existing = await _context.WhatsAppMessages.AsNoTracking()
                        .Where(m => m.CompanyId == companyId && m.ConversationId == conversation.Id)
                        .Where(m => m.Direction == "outbound" && m.Source == "widget")
                        .Where(m => m.TwilioSid == dto.ClientMessageId)
                        .FirstOrDefaultAsync();
                }
                if (existing == null)
                {
                    var dedupeSince = DateTime.UtcNow.AddMinutes(-2);
                    existing = await _context.WhatsAppMessages.AsNoTracking()
                        .Where(m => m.CompanyId == companyId && m.ConversationId == conversation.Id)
                        .Where(m => m.Direction == "outbound" && m.Source == "widget")
                        .Where(m => m.Timestamp >= dedupeSince)
                        .Where(m => m.Body == dto.Message)
                        .FirstOrDefaultAsync();
                }

                if (existing != null)
                {
                    _logger.LogInformation("Duplicate widget response suppressed for conversation {ConversationId}", conversationId);
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Duplicate suppressed",
                        Data = new { messageId = existing.Id, timestamp = existing.Timestamp }
                    });
                }

                // If a new sessionId was provided, update conversation to bind to active widget session
                if (!string.IsNullOrWhiteSpace(dto.SessionId) && conversation.SessionId != dto.SessionId)
                {
                    conversation.SessionId = dto.SessionId!;
                }

                // Create the response message
                var message = new WhatsAppMessage
                {
                    Id = Guid.NewGuid(),
                    // Use client-provided id if available for idempotency, else random
                    TwilioSid = !string.IsNullOrWhiteSpace(dto.ClientMessageId)
                        ? dto.ClientMessageId!.Length > 100 ? dto.ClientMessageId!.Substring(0, 100) : dto.ClientMessageId!
                        : $"WDR{Guid.NewGuid().ToString("N").Substring(0, 12)}",
                    From = "business",
                    To = conversation.CustomerPhone,
                    Body = dto.Message,
                    MessageType = dto.MessageType,
                    Direction = "outbound",
                    Status = "sent",
                    ConversationId = conversation.Id,
                    CompanyId = companyId,
                    Source = "widget",
                    SessionId = conversation.SessionId, // now updated if dto.SessionId provided
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        agentName = dto.AgentName,
                        messageType = dto.MessageType
                    })
                };

                _context.WhatsAppMessages.Add(message);

                // Update conversation
                conversation.MessageCount++;
                conversation.LastMessagePreview = dto.Message.Length > 100 ? dto.Message.Substring(0, 100) + "..." : dto.Message;
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.LastMessageSender = "business";
                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Response sent to widget conversation {ConversationId}", conversationId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Response sent successfully",
                    Data = new
                    {
                        messageId = message.Id,
                        timestamp = message.Timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending response to widget");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error sending response"
                });
            }
        }

        /// <summary>
        /// Get messages for a widget session (for polling)
        /// </summary>
        [HttpGet("session/{sessionId}/messages")]
        public async Task<IActionResult> GetWidgetMessages(string sessionId, [FromQuery] DateTime? since)
        {
            try
            {
                var companyId = GetCompanyId();
                
                var query = _context.WhatsAppMessages
                    .Where(m => m.SessionId == sessionId && 
                           m.CompanyId == companyId);  // Return all messages for the session

                if (since.HasValue)
                {
                    query = query.Where(m => m.Timestamp > since.Value);
                }

                var rawMessages = await query
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
                
                var messages = rawMessages.Select(m => {
                    string agentName = null;
                    if (m.Metadata != null)
                    {
                        try
                        {
                            var doc = JsonDocument.Parse(m.Metadata);
                            if (doc.RootElement.TryGetProperty("agentName", out var prop))
                            {
                                agentName = prop.GetString();
                            }
                        }
                        catch { }
                    }
                    
                    return new
                    {
                        id = m.Id,
                        body = m.Body,
                        isFromMe = m.Direction == "outbound",
                        timestamp = m.Timestamp,
                        status = m.Status,
                        agentName = agentName
                    };
                }).ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Messages retrieved",
                    Data = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving widget messages");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving messages"
                });
            }
        }

        /// <summary>
        /// Close a widget conversation
        /// </summary>
        [HttpPost("conversation/{conversationId}/close")]
        public async Task<IActionResult> CloseWidgetConversation(Guid conversationId, [FromBody] WidgetConversationStatusDto dto)
        {
            try
            {
                var companyId = GetCompanyId();

                var conversation = await _context.WhatsAppConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    });
                }

                // Update conversation status
                conversation.Status = dto.Status;
                conversation.UpdatedAt = DateTime.UtcNow;

                // Add closing message if provided
                if (!string.IsNullOrEmpty(dto.ClosingMessage))
                {
                    var closingMessage = new WhatsAppMessage
                    {
                        Id = Guid.NewGuid(),
                        TwilioSid = $"widget_system_{Guid.NewGuid()}",
                        From = "system",
                        To = conversation.CustomerPhone,
                        Body = dto.ClosingMessage,
                        MessageType = "system",
                        Direction = "outbound",
                        Status = "sent",
                        ConversationId = conversation.Id,
                        CompanyId = companyId,
                        Source = "widget",
                        SessionId = conversation.SessionId,
                        Timestamp = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Metadata = JsonSerializer.Serialize(new
                        {
                            messageType = "closing",
                            status = dto.Status
                        })
                    };

                    _context.WhatsAppMessages.Add(closingMessage);
                    conversation.LastMessagePreview = dto.ClosingMessage;
                    conversation.LastMessageAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Widget conversation {ConversationId} closed with status {Status}", 
                    conversationId, dto.Status);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Conversation closed successfully",
                    Data = new
                    {
                        conversationId = conversation.Id,
                        status = conversation.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing widget conversation");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error closing conversation"
                });
            }
        }

        private int GetCompanyId()
        {
            // Try to get from subdomain or token
            // For now, return default
            return 1;
        }
    }
}
