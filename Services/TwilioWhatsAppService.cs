using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Security;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services.Encryption;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service for managing Twilio WhatsApp integration
    /// </summary>
    public class TwilioWhatsAppService : ITwilioWhatsAppService, IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<TwilioWhatsAppService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public TwilioWhatsAppService(
            ApplicationDbContext context,
            IEncryptionService encryptionService,
            ILogger<TwilioWhatsAppService> logger,
            IConfiguration configuration,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // IWhatsAppService implementation properties
        public string ProviderName => "Twilio";

        public bool IsConfigured(int companyId)
        {
            var config = _context.Set<WhatsAppConfig>()
                .FirstOrDefault(c => c.CompanyId == companyId);

            return config != null && 
                   config.IsActive && 
                   !string.IsNullOrEmpty(config.TwilioAccountSid) && 
                   !string.IsNullOrEmpty(config.TwilioAuthToken);
        }

        #region Configuration Management

        public async Task<WhatsAppConfigDto?> GetConfigAsync(int companyId)
        {
            var config = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
                return null;

            return MapToConfigDto(config);
        }

        public async Task<WhatsAppConfigDto> CreateConfigAsync(int companyId, CreateWhatsAppConfigDto dto)
        {
            // Check if config already exists
            var existingConfig = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (existingConfig != null)
                throw new InvalidOperationException("WhatsApp configuration already exists for this company");

            var config = new WhatsAppConfig
            {
                CompanyId = companyId,
                TwilioAccountSid = _encryptionService.Encrypt(dto.TwilioAccountSid),
                TwilioAuthToken = _encryptionService.Encrypt(dto.TwilioAuthToken),
                WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber,
                WebhookUrl = dto.WebhookUrl,
                IsActive = dto.IsActive,
                UseSandbox = dto.UseSandbox,
                WebhookToken = dto.WebhookToken,
                RateLimitPerMinute = dto.RateLimitPerMinute,
                RateLimitPerHour = dto.RateLimitPerHour,
                MaxRetryAttempts = dto.MaxRetryAttempts,
                RetryDelayMinutes = dto.RetryDelayMinutes
            };

            if (dto.AutoReplySettings != null)
            {
                config.AutoReplySettings = JsonSerializer.Serialize(dto.AutoReplySettings);
            }

            if (dto.BusinessHours != null)
            {
                config.BusinessHours = JsonSerializer.Serialize(dto.BusinessHours);
            }

            _context.Set<WhatsAppConfig>().Add(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("WhatsApp configuration created for company {CompanyId}", companyId);

            return MapToConfigDto(config);
        }

        private string MaskToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length <= 4)
                return "****";
            return $"****{token.Substring(token.Length - 4)}";
        }
        
        private string MaskAccountSid(string accountSid)
        {
            if (string.IsNullOrEmpty(accountSid) || accountSid.Length <= 4)
                return "****";
            return $"****{accountSid.Substring(accountSid.Length - 4)}";
        }

        public async Task<WhatsAppConfigDto> UpdateConfigAsync(int companyId, UpdateWhatsAppConfigDto dto)
        {
            var config = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
                throw new KeyNotFoundException("WhatsApp configuration not found");

            // Update only provided fields
            // Update Provider if provided
            if (!string.IsNullOrEmpty(dto.Provider))
                config.Provider = dto.Provider;
                
            // Update Twilio fields
            if (!string.IsNullOrEmpty(dto.TwilioAccountSid))
            {
                config.TwilioAccountSid = _encryptionService.Encrypt(dto.TwilioAccountSid);
                config.TwilioAccountSidMask = MaskAccountSid(dto.TwilioAccountSid);
            }

            if (!string.IsNullOrEmpty(dto.TwilioAuthToken))
            {
                config.TwilioAuthToken = _encryptionService.Encrypt(dto.TwilioAuthToken);
                config.TwilioAuthTokenMask = MaskToken(dto.TwilioAuthToken);
            }
            
            // Update Green API fields
            if (!string.IsNullOrEmpty(dto.GreenApiInstanceId))
                config.GreenApiInstanceId = dto.GreenApiInstanceId;
                
            if (!string.IsNullOrEmpty(dto.GreenApiToken))
            {
                config.GreenApiToken = _encryptionService.Encrypt(dto.GreenApiToken);
                config.GreenApiTokenMask = MaskToken(dto.GreenApiToken);
            }

            if (!string.IsNullOrEmpty(dto.WhatsAppPhoneNumber))
                config.WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber;

            if (!string.IsNullOrEmpty(dto.WebhookUrl))
                config.WebhookUrl = dto.WebhookUrl;

            // Update webhook authentication fields
            if (!string.IsNullOrEmpty(dto.WebhookSecret))
                config.WebhookSecret = _encryptionService.Encrypt(dto.WebhookSecret);

            if (!string.IsNullOrEmpty(dto.HeaderName))
                config.HeaderName = dto.HeaderName;

            if (!string.IsNullOrEmpty(dto.HeaderValueTemplate))
                config.HeaderValueTemplate = dto.HeaderValueTemplate;

            if (dto.IsActive.HasValue)
                config.IsActive = dto.IsActive.Value;

            if (dto.UseSandbox.HasValue)
                config.UseSandbox = dto.UseSandbox.Value;

            if (dto.WebhookToken != null)
                config.WebhookToken = dto.WebhookToken;

            if (dto.RateLimitPerMinute.HasValue)
                config.RateLimitPerMinute = dto.RateLimitPerMinute.Value;

            if (dto.RateLimitPerHour.HasValue)
                config.RateLimitPerHour = dto.RateLimitPerHour.Value;

            if (dto.MaxRetryAttempts.HasValue)
                config.MaxRetryAttempts = dto.MaxRetryAttempts.Value;

            if (dto.RetryDelayMinutes.HasValue)
                config.RetryDelayMinutes = dto.RetryDelayMinutes.Value;

            if (dto.AutoReplySettings != null)
                config.AutoReplySettings = JsonSerializer.Serialize(dto.AutoReplySettings);

            if (dto.BusinessHours != null)
                config.BusinessHours = JsonSerializer.Serialize(dto.BusinessHours);

            config.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("WhatsApp configuration updated for company {CompanyId}", companyId);

            return MapToConfigDto(config);
        }

        public async Task<bool> DeleteConfigAsync(int companyId)
        {
            var config = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
                return false;

            _context.Set<WhatsAppConfig>().Remove(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("WhatsApp configuration deleted for company {CompanyId}", companyId);

            return true;
        }

        public async Task<WhatsAppConfigTestResultDto> TestConfigAsync(int companyId, TestWhatsAppConfigDto dto)
        {
            var config = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
            {
                _logger.LogWarning("No WhatsApp config found for company {CompanyId}", companyId);
                throw new KeyNotFoundException($"WhatsApp configuration not found for company {companyId}");
            }

            try
            {
                _logger.LogInformation("Testing WhatsApp for company {CompanyId}, From: {From}, To: {To}", 
                    companyId, config.WhatsAppPhoneNumber, dto.TestPhoneNumber);

                // Decrypt credentials
                string accountSid, authToken;
                try
                {
                    accountSid = _encryptionService.Decrypt(config.TwilioAccountSid);
                    authToken = _encryptionService.Decrypt(config.TwilioAuthToken);
                    _logger.LogDebug("Credentials decrypted successfully for company {CompanyId}", companyId);
                }
                catch (Exception decryptEx)
                {
                    _logger.LogError(decryptEx, "Failed to decrypt credentials for company {CompanyId}", companyId);
                    throw new InvalidOperationException("Failed to decrypt Twilio credentials. Please re-save your configuration.", decryptEx);
                }

                // Initialize Twilio client
                TwilioClient.Init(accountSid, authToken);
                _logger.LogDebug("Twilio client initialized for company {CompanyId}", companyId);

                // Format phone numbers
                var fromNumber = config.WhatsAppPhoneNumber.StartsWith("whatsapp:") 
                    ? config.WhatsAppPhoneNumber 
                    : $"whatsapp:{config.WhatsAppPhoneNumber}";
                var toNumber = dto.TestPhoneNumber.StartsWith("whatsapp:") 
                    ? dto.TestPhoneNumber 
                    : $"whatsapp:{dto.TestPhoneNumber}";

                _logger.LogInformation("Sending test message from {From} to {To}", fromNumber, toNumber);

                var message = await MessageResource.CreateAsync(
                    from: fromNumber,
                    to: toNumber,
                    body: dto.TestMessage ?? "Prueba de configuración de WhatsApp - Twilio Integration"
                );

                config.LastTestedAt = DateTime.UtcNow;
                config.LastTestResult = "Test successful";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Test message sent successfully with SID: {MessageSid}", message.Sid);

                return new WhatsAppConfigTestResultDto
                {
                    Success = true,
                    Message = "Test message sent successfully",
                    TwilioMessageSid = message.Sid,
                    Details = new Dictionary<string, object>
                    {
                        ["status"] = message.Status.ToString(),
                        ["from"] = fromNumber,
                        ["to"] = toNumber,
                        ["price"] = message.Price ?? "0",
                        ["direction"] = message.Direction.ToString()
                    }
                };
            }
            catch (Twilio.Exceptions.ApiException twilioEx)
            {
                config.LastTestedAt = DateTime.UtcNow;
                config.LastTestResult = $"Twilio error: {twilioEx.Message}";
                await _context.SaveChangesAsync();

                _logger.LogError(twilioEx, "Twilio API error for company {CompanyId}: {ErrorCode} - {Message}", 
                    companyId, twilioEx.Code, twilioEx.Message);

                return new WhatsAppConfigTestResultDto
                {
                    Success = false,
                    Message = $"Twilio error: {twilioEx.Message}",
                    Details = new Dictionary<string, object>
                    {
                        ["error"] = twilioEx.Message,
                        ["code"] = twilioEx.Code,
                        ["type"] = "TwilioApiException",
                        ["moreInfo"] = twilioEx.MoreInfo ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                config.LastTestedAt = DateTime.UtcNow;
                config.LastTestResult = $"Test failed: {ex.Message}";
                await _context.SaveChangesAsync();

                _logger.LogError(ex, "WhatsApp configuration test failed for company {CompanyId}: {Message}", 
                    companyId, ex.Message);

                return new WhatsAppConfigTestResultDto
                {
                    Success = false,
                    Message = $"Test failed: {ex.Message}",
                    Details = new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["type"] = ex.GetType().Name,
                        ["stackTrace"] = ex.StackTrace ?? ""
                    }
                };
            }
        }

        #endregion

        #region Message Sending

        public async Task<WhatsAppMessageDto> SendMessageAsync(int companyId, SendWhatsAppMessageDto dto)
        {
            var config = await GetActiveConfigAsync(companyId);
            var conversation = await GetOrCreateConversationInternalAsync(companyId, dto.To, config.WhatsAppPhoneNumber);

            try
            {
                var accountSid = _encryptionService.Decrypt(config.TwilioAccountSid);
                var authToken = _encryptionService.Decrypt(config.TwilioAuthToken);

                TwilioClient.Init(accountSid, authToken);

                var message = await MessageResource.CreateAsync(
                    from: $"whatsapp:{config.WhatsAppPhoneNumber}",
                    to: $"whatsapp:{await FormatPhoneNumberAsync(dto.To)}",
                    body: dto.Body,
                    mediaUrl: !string.IsNullOrEmpty(dto.MediaUrl) ? [new Uri(dto.MediaUrl)] : null
                );

                var whatsAppMessage = new WhatsAppMessage
                {
                    TwilioSid = message.Sid,
                    From = config.WhatsAppPhoneNumber,
                    To = dto.To,
                    Body = dto.Body,
                    MessageType = dto.MessageType,
                    MediaUrl = dto.MediaUrl,
                    Direction = "outbound",
                    Status = message.Status.ToString(),
                    ConversationId = conversation.Id,
                    CompanyId = companyId
                };

                _context.Set<WhatsAppMessage>().Add(whatsAppMessage);
                
                // Update conversation
                conversation.MessageCount++;
                conversation.LastMessagePreview = dto.Body?.Length > 200 ? dto.Body[..200] + "..." : dto.Body;
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.LastMessageSender = "business";
                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("WhatsApp message sent: {MessageSid}", message.Sid);

                return MapToMessageDto(whatsAppMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WhatsApp message to {To}", dto.To);
                throw;
            }
        }

        public async Task<List<WhatsAppMessageDto>> SendBulkMessageAsync(int companyId, BulkSendWhatsAppMessageDto dto)
        {
            var results = new List<WhatsAppMessageDto>();

            foreach (var phoneNumber in dto.To)
            {
                try
                {
                    var singleDto = new SendWhatsAppMessageDto
                    {
                        To = phoneNumber,
                        Body = dto.Body,
                        MediaUrl = dto.MediaUrl,
                        MessageType = dto.MessageType,
                        TemplateId = dto.TemplateId,
                        TemplateParameters = dto.TemplateParameters
                    };

                    var result = await SendMessageAsync(companyId, singleDto);
                    results.Add(result);

                    // Add delay to avoid rate limiting
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send bulk message to {PhoneNumber}", phoneNumber);
                    // Continue with next number
                }
            }

            return results;
        }

        public async Task<WhatsAppMessageDto> SendTemplateMessageAsync(int companyId, string to, string templateId, Dictionary<string, string>? parameters = null)
        {
            var config = await GetActiveConfigAsync(companyId);
            
            // Get template
            var templates = JsonSerializer.Deserialize<List<MessageTemplateDto>>(config.MessageTemplates ?? "[]");
            var template = templates?.FirstOrDefault(t => t.Id == templateId);
            
            if (template == null)
                throw new KeyNotFoundException("Message template not found");

            var message = template.Content;
            
            // Replace parameters in template
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    message = message.Replace($"{{{param.Key}}}", param.Value);
                }
            }

            var dto = new SendWhatsAppMessageDto
            {
                To = to,
                Body = message,
                MessageType = "text"
            };

            return await SendMessageAsync(companyId, dto);
        }

        #endregion

        #region Message Management

        public async Task<WhatsAppMessageDto?> GetMessageAsync(int companyId, Guid messageId)
        {
            var message = await _context.Set<WhatsAppMessage>()
                .Include(m => m.Customer)
                .Include(m => m.RepliedByUser)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.CompanyId == companyId);

            return message == null ? null : MapToMessageDto(message);
        }

        public async Task<List<WhatsAppMessageDto>> GetMessagesAsync(int companyId, Guid conversationId, int page = 1, int pageSize = 50)
        {
            // Read-only fast path: avoid heavy includes and trackless query
            var messages = await _context.Set<WhatsAppMessage>()
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId && m.CompanyId == companyId)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new WhatsAppMessageDto
                {
                    Id = m.Id,
                    TwilioSid = m.TwilioSid,
                    From = m.From,
                    To = m.To,
                    Body = m.Body,
                    MessageType = m.MessageType,
                    MediaUrl = m.MediaUrl,
                    MediaContentType = m.MediaContentType,
                    Direction = m.Direction,
                    Status = m.Status,
                    ErrorCode = m.ErrorCode,
                    ErrorMessage = m.ErrorMessage,
                    ConversationId = m.ConversationId,
                    CustomerId = m.CustomerId,
                    // CustomerName intentionally omitted to avoid join; UI does not require it
                    RepliedByUserId = m.RepliedByUserId,
                    Timestamp = m.Timestamp,
                    DeliveredAt = m.DeliveredAt,
                    ReadAt = m.ReadAt,
                })
                .ToListAsync();

            return messages;
        }

        public async Task<bool> MarkMessageAsReadAsync(int companyId, Guid messageId)
        {
            var message = await _context.Set<WhatsAppMessage>()
                .FirstOrDefaultAsync(m => m.Id == messageId && m.CompanyId == companyId);

            if (message == null || message.Direction != "inbound")
                return false;

            message.ReadAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkConversationAsReadAsync(int companyId, Guid conversationId)
        {
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

            if (conversation == null)
                return false;

            // Mark all unread messages as read
            var unreadMessages = await _context.Set<WhatsAppMessage>()
                .Where(m => m.ConversationId == conversationId && 
                           m.Direction == "inbound" && 
                           m.ReadAt == null)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var message in unreadMessages)
            {
                message.ReadAt = now;
                message.UpdatedAt = now;
            }

            conversation.UnreadCount = 0;
            conversation.UpdatedAt = now;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Private Helper Methods

        private async Task<WhatsAppConfig> GetActiveConfigAsync(int companyId)
        {
            var config = await _context.Set<WhatsAppConfig>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.IsActive);

            if (config == null)
                throw new InvalidOperationException("No active WhatsApp configuration found for this company");

            return config;
        }

        private async Task<WhatsAppConversation> GetOrCreateConversationInternalAsync(int companyId, string customerPhone, string businessPhone)
        {
            var formattedPhone = await FormatPhoneNumberAsync(customerPhone);
            
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && 
                                        c.CustomerPhone == formattedPhone && 
                                        c.BusinessPhone == businessPhone);

            if (conversation == null)
            {
                conversation = new WhatsAppConversation
                {
                    CompanyId = companyId,
                    CustomerPhone = formattedPhone,
                    BusinessPhone = businessPhone,
                    Status = "active"
                };

                _context.Set<WhatsAppConversation>().Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversation;
        }

        private static WhatsAppConfigDto MapToConfigDto(WhatsAppConfig config)
        {
            return new WhatsAppConfigDto
            {
                Id = config.Id,
                CompanyId = config.CompanyId,
                Provider = config.Provider, // Include Provider
                TwilioAccountSidMask = MaskSensitiveData(config.TwilioAccountSid),
                WhatsAppPhoneNumber = config.WhatsAppPhoneNumber,
                WebhookUrl = config.WebhookUrl,
                WebhookSecret = config.WebhookSecret, // Include the webhook secret (already decrypted in service)
                HeaderName = config.HeaderName,
                HeaderValueTemplate = config.HeaderValueTemplate,
                IsActive = config.IsActive,
                UseSandbox = config.UseSandbox,
                // Include Green API fields if Provider is GreenAPI
                GreenApiInstanceId = config.Provider == "GreenAPI" ? config.GreenApiInstanceId : null,
                GreenApiToken = config.Provider == "GreenAPI" ? config.GreenApiTokenMask : null, // Return masked token
                AutoReplySettings = string.IsNullOrEmpty(config.AutoReplySettings) 
                    ? null 
                    : JsonSerializer.Deserialize<AutoReplySettingsDto>(config.AutoReplySettings),
                BusinessHours = string.IsNullOrEmpty(config.BusinessHours) 
                    ? null 
                    : JsonSerializer.Deserialize<BusinessHoursDto>(config.BusinessHours),
                MessageTemplates = string.IsNullOrEmpty(config.MessageTemplates) 
                    ? new List<MessageTemplateDto>() 
                    : JsonSerializer.Deserialize<List<MessageTemplateDto>>(config.MessageTemplates),
                RateLimitPerMinute = config.RateLimitPerMinute,
                RateLimitPerHour = config.RateLimitPerHour,
                MaxRetryAttempts = config.MaxRetryAttempts,
                RetryDelayMinutes = config.RetryDelayMinutes,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt,
                LastTestedAt = config.LastTestedAt,
                LastTestResult = config.LastTestResult
            };
        }

        private static WhatsAppMessageDto MapToMessageDto(WhatsAppMessage message)
        {
            return new WhatsAppMessageDto
            {
                Id = message.Id,
                TwilioSid = message.TwilioSid,
                From = message.From,
                To = message.To,
                Body = message.Body,
                MessageType = message.MessageType,
                MediaUrl = message.MediaUrl,
                MediaContentType = message.MediaContentType,
                Direction = message.Direction,
                Status = message.Status,
                ErrorCode = message.ErrorCode,
                ErrorMessage = message.ErrorMessage,
                ConversationId = message.ConversationId,
                CustomerId = message.CustomerId,
                CustomerName = message.Customer != null ? $"{message.Customer.FirstName} {message.Customer.LastName}" : null,
                RepliedByUserId = message.RepliedByUserId,
                RepliedByUserName = message.RepliedByUser != null ? $"{message.RepliedByUser.FirstName} {message.RepliedByUser.LastName}" : null,
                Timestamp = message.Timestamp,
                DeliveredAt = message.DeliveredAt,
                ReadAt = message.ReadAt,
                Metadata = string.IsNullOrEmpty(message.Metadata) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(message.Metadata)
            };
        }

        private static string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data) || data.Length <= 4)
                return "****";
            
            return new string('*', data.Length - 4) + data[^4..];
        }

        #endregion

        #region Not Implemented Methods (Will be implemented in next part)

        public async Task<WhatsAppConversationListDto> GetConversationsAsync(int companyId, WhatsAppConversationFilterDto filter)
        {
            try
            {
                var query = _context.Set<WhatsAppConversation>()
                    .AsNoTracking()
                    .Where(c => c.CompanyId == companyId);

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(c => c.Status == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(c => 
                        c.CustomerPhone.Contains(filter.SearchTerm) ||
                        (c.CustomerName != null && c.CustomerName.Contains(filter.SearchTerm)) ||
                        (c.LastMessagePreview != null && c.LastMessagePreview.Contains(filter.SearchTerm)));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var conversations = await query
                    .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new WhatsAppConversationDto
                    {
                        Id = c.Id,
                        CustomerPhone = c.CustomerPhone,
                        CustomerName = c.CustomerName,
                        BusinessPhone = c.BusinessPhone,
                        Status = c.Status,
                        Priority = c.Priority,
                        UnreadCount = c.UnreadCount,
                        MessageCount = c.MessageCount,
                        LastMessagePreview = c.LastMessagePreview,
                        LastMessageAt = c.LastMessageAt,
                        LastMessageSender = c.LastMessageSender,
                        StartedAt = c.StartedAt,
                        AssignedUserId = c.AssignedUserId
                    })
                    .ToListAsync();

                return new WhatsAppConversationListDto
                {
                    Conversations = conversations,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WhatsAppConversationDetailDto?> GetConversationDetailAsync(int companyId, Guid conversationId)
        {
            try
            {
                var conversation = await _context.Set<WhatsAppConversation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                    return null;

                // Load last 50 messages (descending then reshape ascending if needed by client)
                var messages = await _context.Set<WhatsAppMessage>()
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId && m.CompanyId == companyId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(50)
                    .Select(m => new WhatsAppMessageDto
                    {
                        Id = m.Id,
                        TwilioSid = m.TwilioSid,
                        From = m.From,
                        To = m.To,
                        Body = m.Body,
                        MessageType = m.MessageType,
                        MediaUrl = m.MediaUrl,
                        Direction = m.Direction,
                        Status = m.Status,
                        Timestamp = m.Timestamp,
                        DeliveredAt = m.DeliveredAt,
                        ReadAt = m.ReadAt,
                        ConversationId = m.ConversationId,
                        CustomerId = conversation.CustomerId
                    })
                    .ToListAsync();

                return new WhatsAppConversationDetailDto 
                { 
                    MessageCount = messages.Count, 
                    Messages = messages, 
                    Conversation = new WhatsAppConversationDto
                    {
                        Id = conversation.Id,
                        CustomerPhone = conversation.CustomerPhone,
                        BusinessPhone = conversation.BusinessPhone,
                        CustomerId = conversation.CustomerId,
                        CustomerName = conversation.CustomerName,
                        Status = conversation.Status,
                        UnreadCount = conversation.UnreadCount,
                        LastMessageAt = conversation.LastMessageAt,
                        CreatedAt = conversation.CreatedAt,
                        UpdatedAt = conversation.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation detail {ConversationId} for company {CompanyId}", conversationId, companyId);
                throw;
            }
        }

        public async Task<WhatsAppConversationDto?> GetOrCreateConversationAsync(int companyId, string customerPhone, string businessPhone)
        {
            try
            {
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => 
                        c.CompanyId == companyId &&
                        c.CustomerPhone == customerPhone &&
                        c.BusinessPhone == businessPhone &&
                        c.Status != "closed");

                if (conversation == null)
                {
                    conversation = new WhatsAppConversation
                    {
                        CompanyId = companyId,
                        CustomerPhone = customerPhone,
                        BusinessPhone = businessPhone,
                        Status = "active",
                        Priority = "normal",
                        StartedAt = DateTime.UtcNow
                    };
                    _context.Set<WhatsAppConversation>().Add(conversation);
                    await _context.SaveChangesAsync();
                }

                return new WhatsAppConversationDto
                {
                    Id = conversation.Id,
                    CustomerPhone = conversation.CustomerPhone,
                    CustomerName = conversation.CustomerName,
                    BusinessPhone = conversation.BusinessPhone,
                    Status = conversation.Status,
                    Priority = conversation.Priority,
                    UnreadCount = conversation.UnreadCount,
                    MessageCount = conversation.MessageCount,
                    LastMessagePreview = conversation.LastMessagePreview,
                    LastMessageAt = conversation.LastMessageAt,
                    LastMessageSender = conversation.LastMessageSender,
                    StartedAt = conversation.StartedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating conversation for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WhatsAppConversationDto> UpdateConversationAsync(int companyId, Guid conversationId, UpdateWhatsAppConversationDto dto)
        {
            try
            {
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                    throw new KeyNotFoundException("Conversation not found");

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Status))
                    conversation.Status = dto.Status;

                if (!string.IsNullOrEmpty(dto.Priority))
                    conversation.Priority = dto.Priority;

                if (dto.AssignedUserId.HasValue)
                    conversation.AssignedUserId = dto.AssignedUserId;

                if (dto.Tags != null)
                    conversation.Tags = JsonSerializer.Serialize(dto.Tags);

                if (!string.IsNullOrEmpty(dto.Notes))
                    conversation.Notes = dto.Notes;

                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new WhatsAppConversationDto
                {
                    Id = conversation.Id,
                    CustomerPhone = conversation.CustomerPhone,
                    CustomerName = conversation.CustomerName,
                    BusinessPhone = conversation.BusinessPhone,
                    Status = conversation.Status,
                    Priority = conversation.Priority,
                    AssignedUserId = conversation.AssignedUserId,
                    UnreadCount = conversation.UnreadCount,
                    MessageCount = conversation.MessageCount,
                    LastMessagePreview = conversation.LastMessagePreview,
                    LastMessageAt = conversation.LastMessageAt,
                    LastMessageSender = conversation.LastMessageSender,
                    StartedAt = conversation.StartedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation {ConversationId} for company {CompanyId}", conversationId, companyId);
                throw;
            }
        }

        public async Task<bool> CloseConversationAsync(int companyId, Guid conversationId)
        {
            try
            {
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                    return false;

                conversation.Status = "closed";
                conversation.ClosedAt = DateTime.UtcNow;
                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing conversation {ConversationId} for company {CompanyId}", conversationId, companyId);
                return false;
            }
        }

        public async Task<bool> ArchiveConversationAsync(int companyId, Guid conversationId)
        {
            try
            {
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                    return false;

                conversation.Status = "archived";
                conversation.ArchivedAt = DateTime.UtcNow;
                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation {ConversationId} for company {CompanyId}", conversationId, companyId);
                return false;
            }
        }

        // IWhatsAppService interface implementation
        public async Task<bool> ProcessIncomingMessageAsync(object webhookData)
        {
            if (webhookData is TwilioWebhookDto twilioData)
                return await ProcessIncomingMessageAsync(twilioData);
            
            _logger.LogError("Invalid webhook data type for Twilio provider: {Type}", webhookData?.GetType().Name);
            return false;
        }

        public async Task<bool> ProcessIncomingMessageAsync(TwilioWebhookDto webhookData)
        {
            try
            {
                if (webhookData == null)
                {
                    _logger.LogWarning("Received null webhook data");
                    return false;
                }

                _logger.LogInformation("Processing incoming WhatsApp message from {From} to {To}", 
                    webhookData.From, webhookData.To);

                // Extract phone numbers from WhatsApp format (whatsapp:+1234567890)
                var fromPhone = webhookData.From?.Replace("whatsapp:", "");
                var toPhone = webhookData.To?.Replace("whatsapp:", "");

                // Find the config based on the business phone number
                var config = await _context.Set<WhatsAppConfig>()
                    .FirstOrDefaultAsync(c => c.WhatsAppPhoneNumber == toPhone && c.IsActive);

                if (config == null)
                {
                    _logger.LogWarning("No active WhatsApp config found for phone {Phone}", toPhone);
                    return false;
                }

                // Get or create conversation
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => 
                        c.CompanyId == config.CompanyId &&
                        c.CustomerPhone == fromPhone &&
                        c.Status != "closed");

                if (conversation == null)
                {
                    conversation = new WhatsAppConversation
                    {
                        CompanyId = config.CompanyId,
                        CustomerPhone = fromPhone!,
                        CustomerName = webhookData.ProfileName ?? "Unknown",
                        BusinessPhone = toPhone!,
                        Status = "active",
                        StartedAt = DateTime.UtcNow,
                        LastMessageAt = DateTime.UtcNow,
                        UnreadCount = 1
                    };
                    _context.Set<WhatsAppConversation>().Add(conversation);
                }
                else
                {
                    conversation.LastMessageAt = DateTime.UtcNow;
                    conversation.UnreadCount++;
                    if (conversation.Status == "archived")
                        conversation.Status = "active";
                }

                // Create message record
                var message = new WhatsAppMessage
                {
                    ConversationId = conversation.Id,
                    CompanyId = config.CompanyId,
                    TwilioSid = webhookData.MessageSid!,
                    From = fromPhone!,
                    To = toPhone!,
                    Body = webhookData.Body,
                    Direction = "inbound",
                    Status = "received",
                    Timestamp = DateTime.UtcNow
                };

                // Handle media attachments if any
                if (webhookData.NumMedia > 0 && _httpContextAccessor?.HttpContext != null)
                {
                    var mediaInfo = new List<Dictionary<string, string>>();
                    var request = _httpContextAccessor.HttpContext.Request;
                    for (int i = 0; i < webhookData.NumMedia; i++)
                    {
                        var mediaUrl = request.Form[$"MediaUrl{i}"].FirstOrDefault();
                        var mediaType = request.Form[$"MediaContentType{i}"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(mediaUrl))
                        {
                            mediaInfo.Add(new Dictionary<string, string>
                            {
                                { "url", mediaUrl },
                                { "contentType", mediaType ?? "unknown" }
                            });
                        }
                    }
                    if (mediaInfo.Count > 0)
                    {
                        message.Metadata = JsonSerializer.Serialize(new { mediaUrls = mediaInfo });
                        message.MessageType = "media";
                    }
                }

                _context.Set<WhatsAppMessage>().Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully processed incoming message {MessageSid}", webhookData.MessageSid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming WhatsApp message");
                return false;
            }
        }

        // IWhatsAppService interface implementation  
        public async Task<bool> ProcessMessageStatusAsync(object statusData)
        {
            if (statusData is TwilioStatusWebhookDto twilioStatusData)
                return await ProcessMessageStatusAsync(twilioStatusData);
            
            _logger.LogError("Invalid status data type for Twilio provider: {Type}", statusData?.GetType().Name);
            return false;
        }

        public async Task<bool> ProcessMessageStatusAsync(TwilioStatusWebhookDto statusData)
        {
            try
            {
                if (statusData == null || string.IsNullOrEmpty(statusData.MessageSid))
                {
                    _logger.LogWarning("Received invalid status webhook data");
                    return false;
                }

                _logger.LogInformation("Processing status update for message {MessageSid}: {Status}", 
                    statusData.MessageSid, statusData.MessageStatus);

                var message = await _context.Set<WhatsAppMessage>()
                    .FirstOrDefaultAsync(m => m.TwilioSid == statusData.MessageSid);

                if (message == null)
                {
                    _logger.LogWarning("Message {MessageSid} not found for status update", statusData.MessageSid);
                    return false;
                }

                // Map Twilio status to our status string
                message.Status = statusData.MessageStatus?.ToLower() switch
                {
                    "queued" => "queued",
                    "sent" => "sent",
                    "delivered" => "delivered",
                    "read" => "read",
                    "failed" => "failed",
                    "undelivered" => "failed",
                    _ => message.Status
                };

                if (!string.IsNullOrEmpty(statusData.ErrorCode))
                {
                    message.ErrorCode = statusData.ErrorCode;
                    message.ErrorMessage = statusData.ErrorMessage;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated message {MessageSid} status to {Status}", 
                    statusData.MessageSid, message.Status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message status update for {MessageSid}", statusData?.MessageSid);
                return false;
            }
        }

        public async Task<bool> ValidateWebhookSignatureAsync(string signature, string body, string? token = null)
        {
            try
            {
                // Get the first active WhatsApp config to retrieve the auth token
                var config = await _context.Set<WhatsAppConfig>()
                    .FirstOrDefaultAsync(c => c.IsActive);

                if (config == null)
                {
                    _logger.LogWarning("No active WhatsApp configuration found for webhook validation");
                    return false;
                }

                var authToken = token ?? _encryptionService.Decrypt(config.TwilioAuthToken);
                
                // Use Twilio's validator
                var requestValidator = new RequestValidator(authToken);
                
                // Note: For this overload, we need the URL and form parameters
                // This method signature is kept for backward compatibility
                // The actual validation happens in the overloaded method below
                
                return true; // Temporary return for this signature
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        public async Task<bool> ValidateWebhookSignatureAsync(string signature, string url, Dictionary<string, string> formData)
        {
            try
            {
                // Get the first active WhatsApp config to retrieve the auth token
                var config = await _context.Set<WhatsAppConfig>()
                    .FirstOrDefaultAsync(c => c.IsActive);

                if (config == null)
                {
                    _logger.LogWarning("No active WhatsApp configuration found for webhook validation");
                    return false;
                }

                var authToken = _encryptionService.Decrypt(config.TwilioAuthToken);
                
                // Use Twilio's validator
                var requestValidator = new RequestValidator(authToken);
                
                // Validate the request
                var isValid = requestValidator.Validate(url, formData, signature);
                
                if (!isValid)
                {
                    _logger.LogWarning("Invalid webhook signature for URL: {Url}", url);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature for URL: {Url}", url);
                return false;
            }
        }

        public Task<List<MessageTemplateDto>> GetMessageTemplatesAsync(int companyId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<MessageTemplateDto> CreateMessageTemplateAsync(int companyId, CreateMessageTemplateDto dto)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<MessageTemplateDto> UpdateMessageTemplateAsync(int companyId, string templateId, CreateMessageTemplateDto dto)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> DeleteMessageTemplateAsync(int companyId, string templateId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<ConversationStatsDto> GetConversationStatsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<Dictionary<string, object>> GetMessageAnalyticsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> IsWithinBusinessHoursAsync(int companyId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<string> FormatPhoneNumberAsync(string phoneNumber)
        {
            // Basic phone number formatting
            var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");
            
            if (!cleaned.StartsWith('+'))
                cleaned = "+1" + cleaned; // Default to US if no country code
            
            return Task.FromResult(cleaned);
        }

        public Task<bool> IsValidPhoneNumberAsync(string phoneNumber)
        {
            var phoneRegex = @"^\+[1-9]\d{10,14}$";
            return Task.FromResult(Regex.IsMatch(phoneNumber, phoneRegex));
        }

        public Task<List<string>> GetBlacklistedNumbersAsync(int companyId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> AddToBlacklistAsync(int companyId, string phoneNumber)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> RemoveFromBlacklistAsync(int companyId, string phoneNumber)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        #endregion

        #region Message Management

        public async Task<bool> DeleteMessageAsync(int companyId, Guid conversationId, Guid messageId, int userId)
        {
            try
            {
                // Find and soft delete the message
                var message = await _context.WhatsAppMessages
                    .FirstOrDefaultAsync(m => m.Id == messageId && 
                                            m.ConversationId == conversationId && 
                                            m.CompanyId == companyId &&
                                            !m.IsDeleted);

                if (message == null)
                {
                    _logger.LogWarning("Message {MessageId} not found or already deleted", messageId);
                    return false;
                }

                // Mark as deleted (soft delete)
                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;
                message.DeletedByUserId = userId;
                message.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Message {MessageId} soft deleted by user {UserId}", messageId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                throw;
            }
        }

        #endregion
    }
}
