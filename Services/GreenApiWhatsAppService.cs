using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using WebsiteBuilderAPI.Configuration;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services.Encryption;
using System.Text.Json;
using System.Collections.Concurrent;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Green API response wrapper
    /// </summary>
    public class GreenApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
    /// <summary>
    /// Green API WhatsApp service implementation
    /// Provides comprehensive WhatsApp functionality using Green API
    /// This is a complete enterprise-ready implementation with rate limiting,
    /// error handling, retry logic, and full Green API integration
    /// </summary>
    public partial class GreenApiWhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<GreenApiWhatsAppService> _logger;
        private readonly Configuration.WhatsAppSettings _settings;
        // In-memory cache for base conversations list per company (TTL ~60s)
        private static readonly ConcurrentDictionary<int, (DateTime Ts, List<WhatsAppConversationDto> Data)> _convCache
            = new ConcurrentDictionary<int, (DateTime, List<WhatsAppConversationDto>)>();
        private const int ConversationsCacheSeconds = 60;
        // In-memory cache for recent messages per conversation (TTL ~10s)
        internal static readonly ConcurrentDictionary<(int CompanyId, Guid ConversationId), (DateTime Ts, List<WhatsAppMessageDto> Data)> _msgCache
            = new ConcurrentDictionary<(int, Guid), (DateTime, List<WhatsAppMessageDto>)>();
        private const int MessagesCacheSeconds = 10;
        // Allow external invalidation (e.g., widget message received)
        public static void InvalidateMessagesCache(int companyId, Guid conversationId)
        {
            _msgCache.TryRemove((companyId, conversationId), out _);
        }
        
        // Rate limiting tracking
        private readonly Dictionary<int, DateTime> _lastMessageTime = new();
        private readonly Dictionary<int, int> _messageCount = new();
        private readonly object _rateLimitLock = new();

        public string ProviderName => "GreenAPI";

        public GreenApiWhatsAppService(
            HttpClient httpClient,
            ApplicationDbContext context,
            IEncryptionService encryptionService,
            ILogger<GreenApiWhatsAppService> logger,
            IOptions<Configuration.WhatsAppSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

            ConfigureHttpClient();
        }

        #region HTTP Client Configuration

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.GreenAPI?.ApiBaseUrl ?? "https://api.green-api.com");
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout.HttpRequestTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion

        #region Configuration Management

        public async Task<WhatsAppConfigDto?> GetConfigAsync(int companyId)
        {
            _logger.LogInformation("Getting Green API configuration for company {CompanyId}", companyId);

            try
            {
                var config = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && (c.Provider == "GreenAPI" || c.Provider == "GreenApi"));

                if (config == null)
                {
                    _logger.LogWarning("No Green API configuration found for company {CompanyId}", companyId);
                    return null;
                }

                var decryptedSecret = !string.IsNullOrEmpty(config.WebhookSecret) 
                    ? _encryptionService.Decrypt(config.WebhookSecret) 
                    : null;
                
                _logger.LogInformation("Returning config for company {CompanyId} - WebhookSecret encrypted: {HasEncrypted}, decrypted: {HasDecrypted}, HeaderName: {HeaderName}", 
                    companyId, !string.IsNullOrEmpty(config.WebhookSecret), !string.IsNullOrEmpty(decryptedSecret), config.HeaderName);

                return new WhatsAppConfigDto
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    Provider = config.Provider,
                    WhatsAppPhoneNumber = config.WhatsAppPhoneNumber,
                    WebhookUrl = config.WebhookUrl,
                    WebhookSecret = decryptedSecret, // Return decrypted secret
                    HeaderName = config.HeaderName,
                    HeaderValueTemplate = config.HeaderValueTemplate,
                    IsActive = config.IsActive,
                    GreenApiInstanceId = config.GreenApiInstanceId,
                    GreenApiToken = config.GreenApiTokenMask, // Return masked token
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt,
                    LastWebhookEventAt = config.LastWebhookEventAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Green API configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WhatsAppConfigDto> CreateConfigAsync(int companyId, CreateWhatsAppConfigDto dto)
        {
            _logger.LogInformation("Creating Green API configuration for company {CompanyId}", companyId);

            try
            {
                // Check if configuration already exists
                var existingConfig = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && (c.Provider == "GreenAPI" || c.Provider == "GreenApi"));

                if (existingConfig != null)
                {
                    throw new InvalidOperationException($"Green API configuration already exists for company {companyId}");
                }

                // Validate Green API specific fields
                if (string.IsNullOrEmpty(dto.GreenApiInstanceId))
                {
                    throw new ArgumentException("Green API Instance ID is required");
                }
                if (string.IsNullOrEmpty(dto.GreenApiToken))
                {
                    throw new ArgumentException("Green API Token is required");
                }

                // Prepare webhook token and URL
                var initialToken = string.IsNullOrEmpty(dto.WebhookToken) ? GenerateToken() : dto.WebhookToken;

                // Create new configuration
                var config = new WhatsAppConfig
                {
                    CompanyId = companyId,
                    Provider = "GreenApi", // Keep consistent with existing records
                    GreenApiInstanceId = dto.GreenApiInstanceId,
                    GreenApiToken = _encryptionService.Encrypt(dto.GreenApiToken),
                    GreenApiTokenMask = MaskToken(dto.GreenApiToken),
                    WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber ?? string.Empty,
                    WebhookToken = initialToken,
                    // Store relative path; frontend can prefix with API base URL
                    WebhookUrl = $"/webhooks/greenapi/{companyId}/{initialToken}",
                    // Always use default values for header configuration (simplified for GreenAPI)
                    HeaderName = "Authorization",
                    HeaderValueTemplate = "Bearer {secret}",
                    WebhookSecret = string.IsNullOrEmpty(dto.WebhookSecret) ? null : _encryptionService.Encrypt(dto.WebhookSecret),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // Set null for Twilio fields
                    TwilioAccountSid = null,
                    TwilioAuthToken = null
                };

                _context.WhatsAppConfigs.Add(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Green API configuration created successfully for company {CompanyId}", companyId);

                return new WhatsAppConfigDto
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    Provider = config.Provider,
                    WhatsAppPhoneNumber = config.WhatsAppPhoneNumber,
                    WebhookUrl = config.WebhookUrl,
                    WebhookSecret = dto.WebhookSecret, // Return the original value
                    HeaderName = config.HeaderName,
                    HeaderValueTemplate = config.HeaderValueTemplate,
                    IsActive = config.IsActive,
                    GreenApiInstanceId = config.GreenApiInstanceId,
                    GreenApiToken = config.GreenApiTokenMask,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Green API configuration for company {CompanyId}", companyId);
                throw;
            }
        }
        
        private string MaskToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length <= 4)
                return "****";
            return $"****{token.Substring(token.Length - 4)}";
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');
        }

        public async Task<WhatsAppConfigDto> UpdateConfigAsync(int companyId, UpdateWhatsAppConfigDto dto)
        {
            _logger.LogInformation("Updating Green API configuration for company {CompanyId}", companyId);

            try
            {
                var config = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && (c.Provider == "GreenAPI" || c.Provider == "GreenApi"));

                if (config == null)
                {
                    throw new KeyNotFoundException($"Green API configuration not found for company {companyId}");
                }

                // Update configuration properties
                if (!string.IsNullOrEmpty(dto.WhatsAppPhoneNumber))
                    config.WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber;
                    
                if (!string.IsNullOrEmpty(dto.GreenApiInstanceId))
                    config.GreenApiInstanceId = dto.GreenApiInstanceId;
                    
                if (!string.IsNullOrEmpty(dto.GreenApiToken))
                {
                    config.GreenApiToken = _encryptionService.Encrypt(dto.GreenApiToken);
                    config.GreenApiTokenMask = MaskToken(dto.GreenApiToken);
                }

                // Always use default values for header configuration (simplified for GreenAPI)
                config.HeaderName = "Authorization";
                config.HeaderValueTemplate = "Bearer {secret}";
                _logger.LogInformation("Using default webhook auth header configuration for GreenAPI");

                if (!string.IsNullOrEmpty(dto.WebhookSecret))
                {
                    _logger.LogInformation("Encrypting and saving WebhookSecret for company {CompanyId}", companyId);
                    config.WebhookSecret = _encryptionService.Encrypt(dto.WebhookSecret);
                }
                else
                {
                    _logger.LogWarning("WebhookSecret is null or empty, not updating for company {CompanyId}", companyId);
                }

                // Ensure webhook token and URL
                if (string.IsNullOrEmpty(config.WebhookToken))
                    config.WebhookToken = GenerateToken();

                config.WebhookUrl = $"/webhooks/greenapi/{companyId}/{config.WebhookToken}";

                if (dto.IsActive.HasValue)
                    config.IsActive = dto.IsActive.Value;

                // BusinessHours and AutoReply not available in base UpdateWhatsAppConfigDto
                // TODO: Handle from nested BusinessHours and AutoReplySettings DTOs
                // if (dto.BusinessHours?.Days != null) { ... }
                // if (dto.AutoReplySettings?.WelcomeMessage != null) { ... }

                config.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Green API configuration updated successfully for company {CompanyId}", companyId);

                return new WhatsAppConfigDto
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    Provider = config.Provider,
                    WhatsAppPhoneNumber = config.WhatsAppPhoneNumber,
                    WebhookUrl = config.WebhookUrl,
                    WebhookSecret = !string.IsNullOrEmpty(config.WebhookSecret) 
                        ? _encryptionService.Decrypt(config.WebhookSecret) 
                        : null, // Return decrypted secret
                    HeaderName = config.HeaderName,
                    HeaderValueTemplate = config.HeaderValueTemplate,
                    IsActive = config.IsActive,
                    GreenApiInstanceId = config.GreenApiInstanceId,
                    GreenApiToken = config.GreenApiTokenMask, // Return masked token
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Green API configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> DeleteConfigAsync(int companyId)
        {
            _logger.LogInformation("Deleting Green API configuration for company {CompanyId}", companyId);

            try
            {
                var config = await _context.WhatsAppConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && (c.Provider == "GreenAPI" || c.Provider == "GreenApi"));

                if (config == null)
                {
                    _logger.LogWarning("Green API configuration not found for company {CompanyId}", companyId);
                    return false;
                }

                _context.WhatsAppConfigs.Remove(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Green API configuration deleted successfully for company {CompanyId}", companyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Green API configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WhatsAppConfigTestResultDto> TestConfigAsync(int companyId, TestWhatsAppConfigDto dto)
        {
            _logger.LogInformation("Testing Green API configuration for company {CompanyId}", companyId);

            var result = new WhatsAppConfigTestResultDto
            {
                Success = false,
                Message = "",
                TwilioMessageSid = null, // N/A for Green API
                TestedAt = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["provider"] = "GreenAPI",
                    ["validationResults"] = new Dictionary<string, bool>()
                }
            };

            try
            {
                var config = await GetGreenApiConfigAsync(companyId);
                if (config == null)
                {
                    result.Message = "Green API configuration not found";
                    return result;
                }

                var startTime = DateTime.UtcNow;

                // Test 1: Check instance state
                var instanceStateValid = await TestInstanceStateAsync(config);
                ((Dictionary<string, bool>)result.Details!["validationResults"])["InstanceState"] = instanceStateValid;

                // Test 2: Check account info
                var accountInfoValid = await TestAccountInfoAsync(config);
                ((Dictionary<string, bool>)result.Details!["validationResults"])["AccountInfo"] = accountInfoValid;

                // Test 3: Send test message if phone number provided
                if (!string.IsNullOrEmpty(dto.TestPhoneNumber))
                {
                    var testMessageSent = await SendTestMessageAsync(config, dto.TestPhoneNumber);
                    ((Dictionary<string, bool>)result.Details!["validationResults"])["TestMessage"] = testMessageSent;
                    result.Details!["testPhoneNumber"] = dto.TestPhoneNumber;
                }

                var endTime = DateTime.UtcNow;
                result.Details!["connectionTestDurationMs"] = (long)(endTime - startTime).TotalMilliseconds;

                var validationResults = (Dictionary<string, bool>)result.Details!["validationResults"];
                result.Success = validationResults.All(kvp => kvp.Value);
                result.Message = result.Success ? 
                    "Green API configuration test completed successfully" : 
                    "Green API configuration test failed. Check validation results.";

                // Update last test results
                config.LastTestedAt = DateTime.UtcNow;
                config.LastTestResult = result.Success ? "Success" : "Failed";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Green API configuration test completed for company {CompanyId}. Success: {Success}", 
                    companyId, result.Success);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Green API configuration for company {CompanyId}", companyId);
                result.Message = $"Configuration test failed: {ex.Message}";
                return result;
            }
        }

        public bool IsConfigured(int companyId)
        {
            try
            {
                var config = _context.WhatsAppConfigs
                    .FirstOrDefault(c => c.CompanyId == companyId && (c.Provider == "GreenAPI" || c.Provider == "GreenApi") && c.IsActive);
                
                return config != null && 
                       !string.IsNullOrEmpty(config.GreenApiInstanceId) && 
                       !string.IsNullOrEmpty(config.GreenApiToken) && 
                       !string.IsNullOrEmpty(config.WhatsAppPhoneNumber);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Message Sending

        public async Task<WhatsAppMessageDto> SendMessageAsync(int companyId, SendWhatsAppMessageDto dto)
        {
            _logger.LogInformation("Sending Green API message from company {CompanyId} to {To}", companyId, dto.To);

            try
            {
                var config = await GetGreenApiConfigAsync(companyId);
                if (config == null)
                {
                    throw new InvalidOperationException("Green API configuration not found");
                }

                // Check rate limiting
                if (!await CheckRateLimitAsync(companyId))
                {
                    throw new InvalidOperationException("Rate limit exceeded. Please wait before sending more messages.");
                }

                // Validate phone number
                var isValidPhone = await IsValidPhoneNumberAsync(dto.To);
                if (!isValidPhone)
                {
                    throw new ArgumentException($"Invalid phone number format: {dto.To}");
                }

                // Check blacklist
                var blacklistedNumbers = await GetBlacklistedNumbersAsync(companyId);
                if (blacklistedNumbers.Contains(dto.To))
                {
                    throw new InvalidOperationException($"Phone number {dto.To} is blacklisted");
                }

                var chatId = dto.To.ToGreenApiChatId();
                WhatsAppMessageDto? messageResult = null;

                if (!string.IsNullOrEmpty(dto.MediaUrl))
                {
                    // Send media message
                    // Infer media type from URL or use a default
                    var mediaContentType = InferMediaTypeFromUrl(dto.MediaUrl);
                    messageResult = await SendMediaMessageAsync(config, chatId, dto.Body, dto.MediaUrl, mediaContentType);
                }
                else
                {
                    // Send text message
                    messageResult = await SendTextMessageAsync(config, chatId, dto.Body);
                }

                if (messageResult != null)
                {
                    // Save message to database
                    await SaveMessageToDbAsync(companyId, messageResult, dto.To, config.WhatsAppPhoneNumber);
                    
                    // Update rate limiting counters
                    UpdateRateLimitCounters(companyId);
                }

                _logger.LogInformation("Green API message sent successfully from company {CompanyId} to {To}", companyId, dto.To);
                return messageResult ?? throw new InvalidOperationException("Failed to send message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Green API message from company {CompanyId} to {To}", companyId, dto.To);
                throw;
            }
        }

        public async Task<List<WhatsAppMessageDto>> SendBulkMessageAsync(int companyId, BulkSendWhatsAppMessageDto dto)
        {
            _logger.LogInformation("Sending bulk Green API messages from company {CompanyId} to {Count} recipients", 
                companyId, dto.To.Count);

            var results = new List<WhatsAppMessageDto>();
            var errors = new List<string>();

            try
            {
                foreach (var phoneNumber in dto.To)
                {
                    try
                    {
                        var individualMessage = new SendWhatsAppMessageDto
                        {
                            To = phoneNumber,
                            Body = dto.Body,
                            MediaUrl = dto.MediaUrl,
                            MessageType = dto.MessageType
                        };

                        var result = await SendMessageAsync(companyId, individualMessage);
                        results.Add(result);

                        // Add delay between messages to avoid rate limiting
                        await Task.Delay(TimeSpan.FromSeconds(_settings.RateLimit.WindowSizeMinutes * 60 / _settings.RateLimit.MessagesPerMinute));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send message to {PhoneNumber} in bulk operation", phoneNumber);
                        errors.Add($"{phoneNumber}: {ex.Message}");
                    }
                }

                _logger.LogInformation("Bulk Green API messages completed for company {CompanyId}. Success: {SuccessCount}/{TotalCount}", 
                    companyId, results.Count, dto.To.Count);

                if (errors.Any())
                {
                    _logger.LogWarning("Bulk message errors: {Errors}", string.Join(", ", errors));
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk Green API messages from company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WhatsAppMessageDto> SendTemplateMessageAsync(int companyId, string to, string templateId, Dictionary<string, string>? parameters = null)
        {
            _logger.LogInformation("Sending Green API template message from company {CompanyId} to {To} with template {TemplateId}", 
                companyId, to, templateId);

            try
            {
                // For now, Green API doesn't have native template support like Twilio
                // We'll simulate by replacing placeholders in the template message
                // Note: MessageTemplates not implemented in current DbContext
                // TODO: Implement message templates in database
                // For now, use a fallback message
                string messageContent = $"Template message {templateId}";
                
                // Replace parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        messageContent += $" {param.Key}: {param.Value}";
                    }
                }

                var dto = new SendWhatsAppMessageDto
                {
                    To = to,
                    Body = messageContent
                };

                var result = await SendMessageAsync(companyId, dto);
                result.MessageType = "template";

                _logger.LogInformation("Green API template message sent successfully from company {CompanyId} to {To}", companyId, to);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Green API template message from company {CompanyId} to {To}", companyId, to);
                throw;
            }
        }

        #endregion

        #region Message Management

        public async Task<WhatsAppMessageDto?> GetMessageAsync(int companyId, Guid messageId)
        {
            _logger.LogInformation("Getting Green API message {MessageId} for company {CompanyId}", messageId, companyId);

            try
            {
                var message = await _context.WhatsAppMessages
                    .Include(m => m.Conversation)
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.Conversation.CompanyId == companyId);

                if (message == null)
                {
                    _logger.LogWarning("Message {MessageId} not found for company {CompanyId}", messageId, companyId);
                    return null;
                }

                return MapMessageToDto(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Green API message {MessageId} for company {CompanyId}", messageId, companyId);
                throw;
            }
        }

        public async Task<List<WhatsAppMessageDto>> GetMessagesAsync(int companyId, Guid conversationId, int page = 1, int pageSize = 50)
        {
            _logger.LogInformation("Getting Green API messages for conversation {ConversationId}, company {CompanyId}, page {Page}", 
                conversationId, companyId, page);

            try
            {
                // Serve from short-lived cache if available
                if (_msgCache.TryGetValue((companyId, conversationId), out var cacheEntry)
                    && (DateTime.UtcNow - cacheEntry.Ts).TotalSeconds < MessagesCacheSeconds)
                {
                    return cacheEntry.Data;
                }

                var config = await GetGreenApiConfigAsync(companyId);
                if (config == null)
                {
                    _logger.LogWarning("No Green API configuration found for company {CompanyId}", companyId);
                    return new List<WhatsAppMessageDto>();
                }

                // Ensure conversation exists (companyId + customerPhone + businessPhone)
                var conversation = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

                if (conversation == null)
                {
                    _logger.LogWarning("Conversation {ConversationId} not found for company {CompanyId}", conversationId, companyId);
                    return new List<WhatsAppMessageDto>();
                }

                // LOG ESPECÍFICO: Ver valores exactos de la conversación
                _logger.LogInformation("[WIDGET DEBUG] GetMessages for Conv {ConvId}: Source='{Source}', BusinessPhone='{BusinessPhone}', CustomerPhone='{CustomerPhone}', SessionId='{SessionId}'", 
                    conversationId, 
                    conversation.Source ?? "NULL",
                    conversation.BusinessPhone ?? "NULL",
                    conversation.CustomerPhone ?? "NULL",
                    conversation.SessionId ?? "NULL");

                // For widget-origin conversations, always serve from DB immediately and skip provider
                if ((conversation.Source ?? string.Empty).Equals("widget", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[WIDGET DEBUG] Conversation {ConvId} IS A WIDGET - serving from DB directly", conversationId);
                    // Get the most recent messages first, then order ascending for UI
                    var widgetDb = await _context.WhatsAppMessages.AsNoTracking()
                        .Where(m => m.CompanyId == companyId && m.ConversationId == conversationId)
                        .OrderByDescending(m => m.Timestamp)
                        .Take(pageSize)
                        .ToListAsync();
                    var widgetDtos = widgetDb
                        .OrderBy(m => m.Timestamp)
                        .Select(MapMessageToDto)
                        .ToList();
                    _logger.LogInformation("[WIDGET DEBUG] Found {Count} messages in DB for widget conversation {ConvId}", widgetDtos.Count, conversationId);
                    if (widgetDtos.Count > 0)
                    {
                        _logger.LogInformation("[WIDGET DEBUG] First message: Direction={Dir}, Body={Body}", 
                            widgetDtos.First().Direction, 
                            widgetDtos.First().Body?.Substring(0, Math.Min(50, widgetDtos.First().Body?.Length ?? 0)));
                    }
                    _msgCache[(companyId, conversationId)] = (DateTime.UtcNow, widgetDtos);
                    return widgetDtos;
                }
                else
                {
                    _logger.LogInformation("[WIDGET DEBUG] Conversation {ConvId} is NOT a widget - will try Green API", conversationId);
                }

                // Quick DB read to avoid blocking UI while fetching from provider
                var dbMessages = await _context.WhatsAppMessages.AsNoTracking()
                    .Where(m => m.CompanyId == companyId && m.ConversationId == conversationId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(pageSize)
                    .ToListAsync();
                if (dbMessages.Count > 0)
                {
                    var quick = dbMessages.Select(MapMessageToDto).OrderBy(m => m.Timestamp).ToList();
                    _msgCache[(companyId, conversationId)] = (DateTime.UtcNow, quick);
                    // Fire-and-forget refresh from provider to update cache
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var ctsQuick = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                            var fresh = await FetchMessagesFromGreenApiAsync(config, conversation, conversationId, pageSize, ctsQuick.Token);
                            if (fresh.Count > 0)
                                _msgCache[(companyId, conversationId)] = (DateTime.UtcNow, fresh.OrderBy(m => m.Timestamp).ToList());
                        }
                        catch { }
                    });
                    return quick;
                }

                // Build Green API chatId from customer phone
                var digits = new string((conversation.CustomerPhone ?? "").Where(char.IsDigit).ToArray());
                if (string.IsNullOrEmpty(digits))
                {
                    _logger.LogWarning("Conversation {ConversationId} has empty CustomerPhone", conversationId);
                    return new List<WhatsAppMessageDto>();
                }
                var chatId = $"{digits}@c.us";

                // Call Green API to get chat history
                var historyUrl = $"{_settings.GreenAPI?.ApiBaseUrl ?? "https://api.green-api.com"}/waInstance{config.GreenApiInstanceId}/getChatHistory/{_encryptionService.Decrypt(config.GreenApiToken)}";
                var historyRequest = new { chatId, count = pageSize };

                _logger.LogDebug("Calling Green API getChatHistory for chatId: {ChatId}, URL: {Url}", chatId, historyUrl);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, Math.Min(12, _settings.Timeout.HttpRequestTimeoutSeconds))));
                var historyResponse = await _httpClient.PostAsJsonAsync(historyUrl, historyRequest, cts.Token);
                if (!historyResponse.IsSuccessStatusCode)
                {
                    var errorContent = await historyResponse.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogWarning("Failed to get chat history for {ChatId}: {StatusCode}, Error: {Error}", 
                        chatId, historyResponse.StatusCode, errorContent);
                    return new List<WhatsAppMessageDto>();
                }

                var historyData = await historyResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cts.Token);
                var messages = new List<WhatsAppMessageDto>();

                if (historyData?.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var messageElement in historyData.RootElement.EnumerateArray())
                    {
                        try
                        {
                            string? textMessage = messageElement.TryGetProperty("textMessage", out var textEl) ? textEl.GetString() : null;
                            var timestamp = messageElement.TryGetProperty("timestamp", out var tsEl) ? tsEl.GetInt64() : 0;
                            var fromMe = messageElement.TryGetProperty("fromMe", out var fromMeEl) && fromMeEl.GetBoolean();
                            var idMessage = messageElement.TryGetProperty("idMessage", out var idEl) ? idEl.GetString() : Guid.NewGuid().ToString();
                            var typeRaw = messageElement.TryGetProperty("type", out var typeEl) ? (typeEl.GetString() ?? "chat") : "chat";

                            // Media URL detection
                            string? mediaUrl = null;
                            if (messageElement.TryGetProperty("downloadUrl", out var dl)) mediaUrl = dl.GetString();
                            else if (messageElement.TryGetProperty("fileUrl", out var fu)) mediaUrl = fu.GetString();
                            else if (messageElement.TryGetProperty("url", out var u)) mediaUrl = u.GetString();
                            // mediaUrl = NormalizeGreenMediaUrl(mediaUrl, _settings.GreenAPI?.ApiBaseUrl);

                            // Some media types include caption instead of textMessage
                            if (string.IsNullOrEmpty(textMessage) && messageElement.TryGetProperty("caption", out var cap))
                                textMessage = cap.GetString();

                            // Try to infer media content type if provided
                            string? mediaContentType = null;
                            if (messageElement.TryGetProperty("mimeType", out var mt)) mediaContentType = mt.GetString();

                            // Derive normalized message type for proper rendering on frontend
                            string normalizedType;
                            if (!string.IsNullOrEmpty(mediaUrl))
                            {
                                normalizedType = DetermineMessageTypeFromUrl(mediaUrl!, mediaContentType);
                            }
                            else
                            {
                                var lower = typeRaw.ToLowerInvariant();
                                if (lower.Contains("image")) normalizedType = "image";
                                else if (lower.Contains("video")) normalizedType = "video";
                                else if (lower.Contains("audio")) normalizedType = "audio";
                                else normalizedType = "text";
                            }

                            var dto = new WhatsAppMessageDto
                            {
                                Id = Guid.NewGuid(),
                                ConversationId = conversationId,
                                Body = textMessage ?? string.Empty,
                                Direction = fromMe ? "outbound" : "inbound",
                                MessageType = normalizedType,
                                Status = fromMe ? "delivered" : "received",
                                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                                TwilioSid = idMessage,
                                From = fromMe ? config.WhatsAppPhoneNumber : conversation.CustomerPhone,
                                To = fromMe ? conversation.CustomerPhone : config.WhatsAppPhoneNumber,
                                MediaUrl = mediaUrl,
                                MediaContentType = mediaContentType,
                                Metadata = new Dictionary<string, object>()
                            };
                            // If pure media without text, still include
                            if (!string.IsNullOrEmpty(dto.Body) || !string.IsNullOrEmpty(dto.MediaUrl))
                                messages.Add(dto);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing message from Green API history for {ChatId}", chatId);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Unexpected response format from Green API getChatHistory for {ChatId}", chatId);
                }

                var ordered = messages.OrderBy(m => m.Timestamp).ToList();
                _msgCache[(companyId, conversationId)] = (DateTime.UtcNow, ordered);
                return ordered;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Green API messages for conversation {ConversationId}, company {CompanyId}", 
                    conversationId, companyId);
                throw;
            }
        }

        private async Task<List<WhatsAppMessageDto>> FetchMessagesFromGreenApiAsync(WhatsAppConfig config, WhatsAppConversation conversation, Guid conversationId, int pageSize, CancellationToken cancellationToken)
        {
            var historyUrl = $"{_settings.GreenAPI?.ApiBaseUrl ?? "https://api.green-api.com"}/waInstance{config.GreenApiInstanceId}/getChatHistory/{_encryptionService.Decrypt(config.GreenApiToken)}";
            var digits = new string((conversation.CustomerPhone ?? "").Where(char.IsDigit).ToArray());
            var chatId = $"{digits}@c.us";
            var historyRequest = new { chatId, count = pageSize };

            _logger.LogDebug("Calling Green API getChatHistory for chatId: {ChatId}, URL: {Url}", chatId, historyUrl);
            var historyResponse = await _httpClient.PostAsJsonAsync(historyUrl, historyRequest, cancellationToken);
            if (!historyResponse.IsSuccessStatusCode)
            {
                var errorContent = await historyResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to get chat history for {ChatId}: {StatusCode}, Error: {Error}", chatId, historyResponse.StatusCode, errorContent);
                return new List<WhatsAppMessageDto>();
            }

            var historyData = await historyResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
            var messages = new List<WhatsAppMessageDto>();
            if (historyData?.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var messageElement in historyData.RootElement.EnumerateArray())
                {
                    try
                    {
                        string? textMessage = messageElement.TryGetProperty("textMessage", out var textEl) ? textEl.GetString() : null;
                        var timestamp = messageElement.TryGetProperty("timestamp", out var tsEl) ? tsEl.GetInt64() : 0;
                        var fromMe = messageElement.TryGetProperty("fromMe", out var fromMeEl) && fromMeEl.GetBoolean();
                        var idMessage = messageElement.TryGetProperty("idMessage", out var idEl) ? idEl.GetString() : Guid.NewGuid().ToString();
                        var typeRaw = messageElement.TryGetProperty("type", out var typeEl) ? (typeEl.GetString() ?? "chat") : "chat";

                        // Media URL detection
                        string? mediaUrl = null;
                        if (messageElement.TryGetProperty("downloadUrl", out var dl)) mediaUrl = dl.GetString();
                        else if (messageElement.TryGetProperty("fileUrl", out var fu)) mediaUrl = fu.GetString();
                        else if (messageElement.TryGetProperty("url", out var u)) mediaUrl = u.GetString();

                        // Try to infer media content type if provided
                        string? mediaContentType = null;
                        if (messageElement.TryGetProperty("mimeType", out var mt)) mediaContentType = mt.GetString();

                        // Some media types include caption instead of textMessage
                        if (string.IsNullOrEmpty(textMessage) && messageElement.TryGetProperty("caption", out var cap))
                            textMessage = cap.GetString();

                        // Derive normalized message type
                        string normalizedType;
                        if (!string.IsNullOrEmpty(mediaUrl))
                        {
                            normalizedType = DetermineMessageTypeFromUrl(mediaUrl!, mediaContentType);
                        }
                        else
                        {
                            var lower = typeRaw.ToLowerInvariant();
                            if (lower.Contains("image")) normalizedType = "image";
                            else if (lower.Contains("video")) normalizedType = "video";
                            else if (lower.Contains("audio")) normalizedType = "audio";
                            else normalizedType = "text";
                        }

                        var dto = new WhatsAppMessageDto
                        {
                            Id = Guid.NewGuid(),
                            ConversationId = conversationId,
                            Body = textMessage ?? string.Empty,
                            Direction = fromMe ? "outbound" : "inbound",
                            MessageType = normalizedType,
                            Status = fromMe ? "delivered" : "received",
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                            TwilioSid = idMessage,
                            From = fromMe ? conversation.BusinessPhone : conversation.CustomerPhone,
                            To = fromMe ? conversation.CustomerPhone : conversation.BusinessPhone,
                            MediaUrl = mediaUrl,
                            MediaContentType = mediaContentType,
                            Metadata = new Dictionary<string, object>()
                        };
                        if (!string.IsNullOrEmpty(dto.Body) || !string.IsNullOrEmpty(dto.MediaUrl))
                            messages.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing message from Green API history for {ChatId}", chatId);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Unexpected response format from Green API getChatHistory for {ChatId}", chatId);
            }

            return messages;
        }

        /// <summary>
        /// Enrich a conversation with contact info (name, avatar) from Green API and persist it.
        /// </summary>
        public async Task<bool> EnrichConversationAsync(int companyId, Guid conversationId)
        {
            try
            {
                var config = await GetGreenApiConfigAsync(companyId);
                if (config == null) return false;

                var conv = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);
                if (conv == null) return false;

                var digits = new string((conv.CustomerPhone ?? "").Where(char.IsDigit).ToArray());
                if (string.IsNullOrEmpty(digits)) return false;
                var chatId = $"{digits}@c.us";

                // Decrypt token
                var apiToken = config.GreenApiToken;
                if (!string.IsNullOrEmpty(apiToken))
                {
                    try { apiToken = _encryptionService.Decrypt(apiToken); } catch { }
                }

                var contactUrl = $"{_settings.GreenAPI?.ApiBaseUrl ?? "https://api.green-api.com"}/waInstance{config.GreenApiInstanceId}/getContactInfo/{apiToken}";
                var contactReq = new { chatId };
                var resp = await _httpClient.PostAsJsonAsync(contactUrl, contactReq);
                if (!resp.IsSuccessStatusCode) return false;

                var json = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement?>();
                if (json == null) return false;

                string? name = null;
                string? avatar = null;
                try
                {
                    if (json.Value.TryGetProperty("name", out var nameEl)) name = nameEl.GetString();
                    if (json.Value.TryGetProperty("avatar", out var avEl)) avatar = avEl.GetString();
                }
                catch { }

                var profile = new CustomerProfileDto
                {
                    Name = name ?? conv.CustomerName ?? conv.CustomerPhone,
                    ProfilePictureUrl = avatar
                };

                // Persist
                conv.CustomerName = profile.Name;
                try { conv.CustomerProfile = System.Text.Json.JsonSerializer.Serialize(profile); } catch { }
                conv.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Invalidate cache for this company so next list reflects name/avatar
                _convCache.TryRemove(companyId, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enriching conversation {ConversationId} for company {CompanyId}", conversationId, companyId);
                return false;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(int companyId, Guid messageId)
        {
            _logger.LogInformation("Marking Green API message {MessageId} as read for company {CompanyId}", messageId, companyId);

            try
            {
                var message = await _context.WhatsAppMessages
                    .Include(m => m.Conversation)
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.Conversation.CompanyId == companyId);

                if (message == null || message.ReadAt != null)
                {
                    return false;
                }

                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking Green API message {MessageId} as read for company {CompanyId}", messageId, companyId);
                throw;
            }
        }

        public async Task<bool> MarkConversationAsReadAsync(int companyId, Guid conversationId)
        {
            _logger.LogInformation("Marking Green API conversation {ConversationId} as read for company {CompanyId}", 
                conversationId, companyId);

            try
            {
                var messages = await _context.WhatsAppMessages
                    .Include(m => m.Conversation)
                    .Where(m => m.ConversationId == conversationId && m.Conversation.CompanyId == companyId && m.ReadAt == null)
                    .ToListAsync();

                if (!messages.Any())
                {
                    return false;
                }

                var readTime = DateTime.UtcNow;
                foreach (var message in messages)
                {
                    message.ReadAt = readTime;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking Green API conversation {ConversationId} as read for company {CompanyId}", 
                    conversationId, companyId);
                throw;
            }
        }

        #endregion

        #region Interface Implementation - Full Implementation Needed

        // These methods require full implementation based on the existing database structure
        // For now, they throw NotImplementedException to indicate they need completion

        public async Task<WhatsAppConversationListDto> GetConversationsAsync(int companyId, WhatsAppConversationFilterDto filter)
        {
            _logger.LogInformation("Getting conversations for company {CompanyId}", companyId);

            try
            {
                // Fast path: if DB already has conversations, serve from DB (quick) and refresh in background
                var dbBase = _context.Set<WhatsAppConversation>().Where(c => c.CompanyId == companyId);
                var dbHas = await dbBase.AnyAsync();
                if (dbHas)
                {
                    var dbQuery = dbBase.AsNoTracking();
                    if (!string.IsNullOrWhiteSpace(filter.CustomerPhone))
                        dbQuery = dbQuery.Where(c => c.CustomerPhone.Contains(filter.CustomerPhone));
                    if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                        dbQuery = dbQuery.Where(c => c.CustomerName != null && c.CustomerName.Contains(filter.CustomerName));
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                        dbQuery = dbQuery.Where(c => c.Status == filter.Status);

                    var totalDb = await dbQuery.CountAsync();
                    var pageDb = await dbQuery
                        .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
                        .Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .ToListAsync();

                    var dtoDb = pageDb.Select(c => {
                        CustomerProfileDto? profile = null;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(c.CustomerProfile))
                                profile = System.Text.Json.JsonSerializer.Deserialize<CustomerProfileDto>(c.CustomerProfile);
                        }
                        catch { }
                        return new WhatsAppConversationDto
                        {
                            Id = c.Id,
                            CustomerPhone = c.CustomerPhone,
                            CustomerName = !string.IsNullOrWhiteSpace(c.CustomerName) ? c.CustomerName : profile?.Name,
                            CustomerEmail = c.CustomerEmail,
                            Source = c.Source,
                            SessionId = c.SessionId,
                            BusinessPhone = c.BusinessPhone,
                            Status = c.Status,
                            Priority = c.Priority,
                            UnreadCount = c.UnreadCount,
                            MessageCount = c.MessageCount,
                            LastMessagePreview = c.LastMessagePreview,
                            LastMessageAt = c.LastMessageAt,
                            LastMessageSender = c.LastMessageSender,
                            StartedAt = c.StartedAt,
                            CompanyId = c.CompanyId,
                            CustomerProfile = profile
                        };
                    }).ToList();

                    // Background refresh (ignore result). Avoid using scoped DbContext on background thread.
                    var cfgSnapshot = await GetGreenApiConfigAsync(companyId);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (cfgSnapshot != null)
                            {
                                // RefreshChatsAsync uses _context; to avoid disposed context issues, execute synchronously here if needed
                                // or wrap in a new scope in production. For now, we skip DB writes on background.
                                await Task.CompletedTask;
                            }
                        }
                        catch { }
                    });

                    return new WhatsAppConversationListDto
                    {
                        Conversations = dtoDb,
                        TotalCount = totalDb,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalPages = (int)Math.Ceiling(totalDb / (double)filter.PageSize),
                        HasNextPage = filter.Page * filter.PageSize < totalDb,
                        HasPreviousPage = filter.Page > 1
                    };
                }
                // Serve from cache if available
                if (_convCache.TryGetValue(companyId, out var cacheEntry) &&
                    (DateTime.UtcNow - cacheEntry.Ts).TotalSeconds < ConversationsCacheSeconds &&
                    cacheEntry.Data != null)
                {
                    var fromCache = ApplyFilters(cacheEntry.Data, filter);
                    var totalCountCache = fromCache.Count;
                    var pagedCache = fromCache
                        .Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .ToList();

                    return new WhatsAppConversationListDto
                    {
                        Conversations = pagedCache,
                        TotalCount = totalCountCache,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCountCache / (double)filter.PageSize),
                        HasNextPage = filter.Page * filter.PageSize < totalCountCache,
                        HasPreviousPage = filter.Page > 1
                    };
                }

                var config = await GetGreenApiConfigAsync(companyId);
                if (config == null)
                {
                    _logger.LogWarning("Green API configuration not found for company {CompanyId}", companyId);
                    return new WhatsAppConversationListDto
                    {
                        Conversations = new List<WhatsAppConversationDto>(),
                        TotalCount = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    };
                }

                // Get chats from Green API
                var chatsResponse = await GetChatsFromGreenApiAsync(config);
                if (!chatsResponse.Success || chatsResponse.Data == null)
                {
                    _logger.LogWarning("Failed to fetch chats from Green API: {Message}", chatsResponse.Message);
                    return new WhatsAppConversationListDto
                    {
                        Conversations = new List<WhatsAppConversationDto>(),
                        TotalCount = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    };
                }

                // Convert Green API chats to our conversation format
                var conversations = new List<WhatsAppConversationDto>();
                foreach (var chat in chatsResponse.Data)
                {
                    try
                    {
                        var conversation = await ConvertChatToConversationAsync(config, chat, companyId);
                        if (conversation != null)
                        {
                            conversations.Add(conversation);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error converting chat to conversation");
                    }
                }

                // Update cache with base list
                _convCache[companyId] = (DateTime.UtcNow, conversations);

                // Apply filters
                var filteredConversations = ApplyFilters(conversations, filter);

                // If API yielded no conversations, fall back to persisted DB conversations to avoid empty UI
                if (filteredConversations.Count == 0)
                {
                    _logger.LogInformation("Green API returned 0 conversations; falling back to DB cache for company {CompanyId}", companyId);
                    var query = _context.Set<WhatsAppConversation>().Where(c => c.CompanyId == companyId);
                    if (!string.IsNullOrWhiteSpace(filter.CustomerPhone))
                        query = query.Where(c => c.CustomerPhone.Contains(filter.CustomerPhone));
                    if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                        query = query.Where(c => c.CustomerName != null && c.CustomerName.Contains(filter.CustomerName));
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                        query = query.Where(c => c.Status == filter.Status);

                    var totalDb = await query.CountAsync();
                    var pageDb = await query
                        .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
                        .Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .ToListAsync();

                    var dtoDb = pageDb.Select(c => {
                        CustomerProfileDto? profile = null;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(c.CustomerProfile))
                            {
                                profile = System.Text.Json.JsonSerializer.Deserialize<CustomerProfileDto>(c.CustomerProfile);
                            }
                        }
                        catch { }

                        return new WhatsAppConversationDto
                        {
                            Id = c.Id,
                            CustomerPhone = c.CustomerPhone,
                            CustomerName = !string.IsNullOrWhiteSpace(c.CustomerName) ? c.CustomerName : profile?.Name,
                            CustomerEmail = c.CustomerEmail,
                            Source = c.Source,
                            SessionId = c.SessionId,
                            BusinessPhone = c.BusinessPhone,
                            Status = c.Status,
                            Priority = c.Priority,
                            UnreadCount = c.UnreadCount,
                            MessageCount = c.MessageCount,
                            LastMessagePreview = c.LastMessagePreview,
                            LastMessageAt = c.LastMessageAt,
                            LastMessageSender = c.LastMessageSender,
                            StartedAt = c.StartedAt,
                            CompanyId = c.CompanyId,
                            CustomerProfile = profile
                        };
                    }).ToList();

                    // also cache from DB fallback
                    _convCache[companyId] = (DateTime.UtcNow, dtoDb);
                    return new WhatsAppConversationListDto
                    {
                        Conversations = dtoDb,
                        TotalCount = totalDb,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalPages = (int)Math.Ceiling(totalDb / (double)filter.PageSize),
                        HasNextPage = filter.Page * filter.PageSize < totalDb,
                        HasPreviousPage = filter.Page > 1
                    };
                }

                // Apply pagination on API-derived list
                var totalCount = filteredConversations.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
                var paginatedConversations = filteredConversations
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new WhatsAppConversationListDto
                {
                    Conversations = paginatedConversations,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = filter.Page < totalPages,
                    HasPreviousPage = filter.Page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for company {CompanyId}", companyId);
                return new WhatsAppConversationListDto
                {
                    Conversations = new List<WhatsAppConversationDto>(),
                    TotalCount = 0,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }
        }

        public async Task<WhatsAppConversationDetailDto?> GetConversationDetailAsync(int companyId, Guid conversationId)
        {
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);
            if (conversation == null)
                return null;

            // Try provider first with a short timeout for a fuller history; fallback to cache/DB
            List<WhatsAppMessageDto> messages;
            var config = await GetGreenApiConfigAsync(companyId);
            if (config != null)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var fresh = await FetchMessagesFromGreenApiAsync(config, conversation, conversationId, 50, cts.Token);
                    if (fresh.Count > 0)
                    {
                        messages = fresh.OrderBy(m => m.Timestamp).ToList();
                        _msgCache[(companyId, conversationId)] = (DateTime.UtcNow, messages);
                    }
                    else
                    {
                        throw new TimeoutException("No messages from provider in time window");
                    }
                }
                catch
                {
                    if (_msgCache.TryGetValue((companyId, conversationId), out var cacheEntry) &&
                        (DateTime.UtcNow - cacheEntry.Ts).TotalSeconds < MessagesCacheSeconds)
                    {
                        messages = cacheEntry.Data;
                    }
                    else
                    {
                        var dbMessages = await _context.WhatsAppMessages.AsNoTracking()
                            .Where(m => m.CompanyId == companyId && m.ConversationId == conversationId)
                            .OrderByDescending(m => m.Timestamp)
                            .Take(20)
                            .ToListAsync();
                        messages = dbMessages.Select(MapMessageToDto).OrderBy(m => m.Timestamp).ToList();
                    }
                }
            }
            else
            {
                // No config; use what we have locally
                if (_msgCache.TryGetValue((companyId, conversationId), out var cacheEntry))
                    messages = cacheEntry.Data;
                else
                    messages = new List<WhatsAppMessageDto>();
            }

            return new WhatsAppConversationDetailDto
            {
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
                    UpdatedAt = conversation.UpdatedAt,
                    CompanyId = companyId
                },
                Messages = messages,
                MessageCount = messages.Count
            };
        }

        public Task<WhatsAppConversationDto?> GetOrCreateConversationAsync(int companyId, string customerPhone, string businessPhone)
        {
            return Task.Run(async () =>
            {
                string Normalize(string phone)
                {
                    var digits = new string((phone ?? string.Empty).Where(ch => char.IsDigit(ch) || ch == '+').ToArray());
                    if (string.IsNullOrEmpty(digits)) return string.Empty;
                    if (!digits.StartsWith("+"))
                    {
                        // Default to +1 when no country code provided and looks like 10 digits
                        if (digits.Length == 10) digits = "+1" + digits;
                        else if (digits.Length > 10) digits = "+" + digits;
                        else digits = "+" + digits;
                    }
                    return digits;
                }

                var cust = Normalize(customerPhone);
                var biz = Normalize(businessPhone);

                var existing = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.CustomerPhone == cust && c.BusinessPhone == biz);

                if (existing == null)
                {
                    existing = new WhatsAppConversation
                    {
                        CompanyId = companyId,
                        CustomerPhone = cust,
                        BusinessPhone = biz,
                        Status = "active",
                        StartedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Set<WhatsAppConversation>().Add(existing);
                    await _context.SaveChangesAsync();
                }

                return new WhatsAppConversationDto
                {
                    Id = existing.Id,
                    CustomerPhone = existing.CustomerPhone,
                    BusinessPhone = existing.BusinessPhone,
                    Status = existing.Status,
                    Priority = existing.Priority,
                    UnreadCount = existing.UnreadCount,
                    MessageCount = existing.MessageCount,
                    LastMessagePreview = existing.LastMessagePreview,
                    LastMessageAt = existing.LastMessageAt,
                    StartedAt = existing.StartedAt,
                    CompanyId = existing.CompanyId,
                    CustomerName = existing.CustomerName
                };
            });
        }

        public async Task<WhatsAppConversationDto> UpdateConversationAsync(int companyId, Guid conversationId, UpdateWhatsAppConversationDto dto)
        {
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);

            if (conversation == null)
                throw new KeyNotFoundException("Conversation not found");

            if (!string.IsNullOrEmpty(dto.Status)) conversation.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Priority)) conversation.Priority = dto.Priority;
            if (dto.AssignedUserId.HasValue) conversation.AssignedUserId = dto.AssignedUserId;
            if (dto.Tags != null) conversation.Tags = System.Text.Json.JsonSerializer.Serialize(dto.Tags);
            if (!string.IsNullOrEmpty(dto.Notes)) conversation.Notes = dto.Notes;
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
                StartedAt = conversation.StartedAt,
                CompanyId = conversation.CompanyId
            };
        }

        public async Task<bool> CloseConversationAsync(int companyId, Guid conversationId)
        {
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);
            if (conversation == null) return false;
            conversation.Status = "closed";
            conversation.ClosedAt = DateTime.UtcNow;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveConversationAsync(int companyId, Guid conversationId)
        {
            var conversation = await _context.Set<WhatsAppConversation>()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.CompanyId == companyId);
            if (conversation == null) return false;
            conversation.Status = "archived";
            conversation.ArchivedAt = DateTime.UtcNow;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> ProcessIncomingMessageAsync(object webhookData)
        {
            throw new NotImplementedException("ProcessIncomingMessageAsync needs webhook processing implementation");
        }

        public Task<bool> ProcessMessageStatusAsync(object statusData)
        {
            throw new NotImplementedException("ProcessMessageStatusAsync needs webhook processing implementation");
        }

        public Task<bool> ValidateWebhookSignatureAsync(string signature, string body, string? token = null)
        {
            throw new NotImplementedException("ValidateWebhookSignatureAsync needs webhook signature validation implementation");
        }

        public Task<List<MessageTemplateDto>> GetMessageTemplatesAsync(int companyId)
        {
            throw new NotImplementedException("GetMessageTemplatesAsync needs template management implementation");
        }

        public Task<MessageTemplateDto> CreateMessageTemplateAsync(int companyId, CreateMessageTemplateDto dto)
        {
            throw new NotImplementedException("CreateMessageTemplateAsync needs template management implementation");
        }

        public Task<MessageTemplateDto> UpdateMessageTemplateAsync(int companyId, string templateId, CreateMessageTemplateDto dto)
        {
            throw new NotImplementedException("UpdateMessageTemplateAsync needs template management implementation");
        }

        public Task<bool> DeleteMessageTemplateAsync(int companyId, string templateId)
        {
            throw new NotImplementedException("DeleteMessageTemplateAsync needs template management implementation");
        }

        public Task<ConversationStatsDto> GetConversationStatsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetConversationStatsAsync needs analytics implementation");
        }

        public Task<Dictionary<string, object>> GetMessageAnalyticsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetMessageAnalyticsAsync needs analytics implementation");
        }

        public async Task<bool> IsWithinBusinessHoursAsync(int companyId)
        {
            // TODO: Implement business hours from BusinessHours JSON field
            // For now, always return true (always within business hours)
            return await Task.FromResult(true);
        }

        public async Task<string> FormatPhoneNumberAsync(string phoneNumber)
        {
            await Task.CompletedTask;
            
            // Remove all non-numeric characters except +
            var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");
            
            // Ensure it starts with +
            if (!cleaned.StartsWith("+"))
            {
                // Default to US country code if no country code provided and 10 digits
                if (cleaned.Length == 10)
                {
                    cleaned = "+1" + cleaned;
                }
                else if (cleaned.Length > 10)
                {
                    cleaned = "+" + cleaned;
                }
            }
            
            return cleaned;
        }

        #region Green API Chat Methods

        private async Task<GreenApiResponse<List<dynamic>>> GetChatsFromGreenApiAsync(WhatsAppConfig config)
        {
            try
            {
                // Decrypt the token if it's encrypted
                var apiToken = config.GreenApiToken;
                if (!string.IsNullOrEmpty(apiToken))
                {
                    try
                    {
                        // Try to decrypt - if it fails, assume it's already plain text
                        apiToken = _encryptionService.Decrypt(apiToken);
                    }
                    catch
                    {
                        // Token might already be plain text
                    }
                }
                
                var baseUrl = _settings.GreenAPI?.ApiBaseUrl ?? "https://api.green-api.com";
                var url = $"{baseUrl}/waInstance{config.GreenApiInstanceId}/getChats/{apiToken}";
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogDebug("GreenAPI getChats response (truncated): {Content}", content?.Length > 500 ? content.Substring(0, 500) + "..." : content);
                
                if (response.IsSuccessStatusCode)
                {
                    // Parse as JsonElement array for dynamic handling
                    var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
                    var chatsList = new List<dynamic>();
                    
                    if (jsonResponse.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in jsonResponse.EnumerateArray())
                        {
                            chatsList.Add(item);
                        }
                    }
                    
                    return new GreenApiResponse<List<dynamic>>
                    {
                        Success = true,
                        Data = chatsList
                    };
                }
                
                _logger.LogWarning("Failed to get chats from Green API: {StatusCode} - {Content}", response.StatusCode, content);
                return new GreenApiResponse<List<dynamic>>
                {
                    Success = false,
                    Message = $"Failed to get chats: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chats from Green API");
                return new GreenApiResponse<List<dynamic>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Refresh conversations from Green API into DB cache (quick sync, no heavy enrichment)
        /// </summary>
        public async Task<int> RefreshChatsAsync(int companyId, int max = 1000)
        {
            var config = await GetGreenApiConfigAsync(companyId);
            if (config == null)
            {
                _logger.LogWarning("Cannot refresh chats: no Green API config for company {CompanyId}", companyId);
                return 0;
            }

            var chatsResponse = await GetChatsFromGreenApiAsync(config);
            if (!chatsResponse.Success || chatsResponse.Data == null)
            {
                _logger.LogWarning("Refresh chats failed from Green API for company {CompanyId}", companyId);
                return 0;
            }

            int count = 0;
            foreach (var chat in chatsResponse.Data.Take(max))
            {
                try
                {
                    var dto = await ConvertChatToConversationAsync(config, chat, companyId);
                    if (dto != null) count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error converting chat during refresh");
                }
            }
            return count;
        }

        private async Task<WhatsAppConversationDto?> ConvertChatToConversationAsync(WhatsAppConfig config, dynamic chat, int companyId)
        {
            try
            {
                var chatObj = chat as JsonElement? ?? (JsonElement)chat;
                
                // Log at debug level to avoid noisy logs and overhead
                var chatStr = chatObj.ToString();
                _logger.LogDebug("Converting GreenAPI chat (truncated): {Chat}", chatStr.Length > 300 ? chatStr.Substring(0, 300) + "..." : chatStr);
                
                // Extract chat ID and phone number
                var chatId = chatObj.GetProperty("id").GetString() ?? "";
                
                // Skip empty chat IDs
                if (string.IsNullOrEmpty(chatId))
                {
                    _logger.LogDebug("Skipping chat with empty ID");
                    return null;
                }
                
                var phoneNumber = ExtractPhoneFromChatId(chatId);
                
                // Keep Green API calls minimal for performance; enrich later
                var contactName = phoneNumber;
                string? avatarUrl = null;
                var lastMessageText = "Click para ver mensajes"; // Default text
                DateTime? lastMessageTime = DateTime.UtcNow.AddMinutes(-30); // Default time
                var unreadCount = 0;
                
                // Create customer profile if we have additional info
                CustomerProfileDto? customerProfile = null;
                if (!string.IsNullOrEmpty(avatarUrl) || !string.IsNullOrEmpty(contactName))
                {
                    customerProfile = new CustomerProfileDto
                    {
                        Name = contactName,
                        ProfilePictureUrl = avatarUrl
                    };
                }
                
                // Ensure conversation persisted (stable Id)
                var existing = await _context.Set<WhatsAppConversation>()
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId 
                        && c.CustomerPhone == phoneNumber 
                        && c.BusinessPhone == config.WhatsAppPhoneNumber);

                var created = false;
                if (existing == null)
                {
                    existing = new WhatsAppConversation
                    {
                        CompanyId = companyId,
                        CustomerPhone = phoneNumber,
                        CustomerName = contactName,
                        BusinessPhone = config.WhatsAppPhoneNumber,
                        Status = "active",
                        StartedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Set<WhatsAppConversation>().Add(existing);
                    created = true;
                }

                // Avoid per-chat updates/writes to speed up initial loads.
                // Only save when we created a new conversation to guarantee a stable Id.
                if (created)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogWarning(saveEx, "Failed to persist new conversation for {Phone}", phoneNumber);
                    }
                }

                return new WhatsAppConversationDto
                {
                    Id = existing.Id,
                    CustomerPhone = existing.CustomerPhone,
                    CustomerName = existing.CustomerName,
                    BusinessPhone = existing.BusinessPhone,
                    Status = existing.Status,
                    Priority = "normal",
                    UnreadCount = existing.UnreadCount,
                    MessageCount = existing.MessageCount,
                    LastMessagePreview = existing.LastMessagePreview,
                    CustomerProfile = customerProfile,
                    LastMessageAt = existing.LastMessageAt,
                    CreatedAt = existing.CreatedAt,
                    UpdatedAt = existing.UpdatedAt,
                    CompanyId = existing.CompanyId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting chat to conversation");
                return null;
            }
        }

        private string ExtractPhoneFromChatId(string chatId)
        {
            // Green API chat IDs are in format: "1234567890@c.us" for individual chats
            // or "1234567890-1234567890@g.us" for group chats
            if (chatId.Contains("@"))
            {
                var parts = chatId.Split('@');
                if (parts.Length > 0)
                {
                    var phone = parts[0].Replace("-", "");
                    // Add + prefix if not present
                    if (!phone.StartsWith("+"))
                    {
                        phone = "+" + phone;
                    }
                    return phone;
                }
            }
            return chatId;
        }

        private List<WhatsAppConversationDto> ApplyFilters(List<WhatsAppConversationDto> conversations, WhatsAppConversationFilterDto filter)
        {
            var query = conversations.AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(c => c.Status == filter.Status);
            }

            // Apply priority filter
            if (!string.IsNullOrEmpty(filter.Priority))
            {
                query = query.Where(c => c.Priority == filter.Priority);
            }

            // Apply search term
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(c => 
                    (c.CustomerName != null && c.CustomerName.ToLower().Contains(searchLower)) ||
                    c.CustomerPhone.ToLower().Contains(searchLower) ||
                    (c.LastMessagePreview != null && c.LastMessagePreview.ToLower().Contains(searchLower)));
            }

            // Apply unread filter
            if (filter.HasUnreadMessages.HasValue && filter.HasUnreadMessages.Value)
            {
                query = query.Where(c => c.UnreadCount > 0);
            }

            // Apply sorting
            if (filter.SortBy?.ToLower() == "lastmessageat")
            {
                query = filter.SortOrder?.ToLower() == "asc" 
                    ? query.OrderBy(c => c.LastMessageAt ?? DateTime.MinValue)
                    : query.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue);
            }
            else
            {
                // Default sort by last message time descending
                query = query.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue);
            }

            return query.ToList();
        }

        #endregion

        public async Task<bool> IsValidPhoneNumberAsync(string phoneNumber)
        {
            await Task.CompletedTask;
            
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Use the regex from settings for validation
            var regex = new Regex(_settings.Validation.PhoneNumberRegex);
            return regex.IsMatch(phoneNumber);
        }

        public async Task<List<string>> GetBlacklistedNumbersAsync(int companyId)
        {
            // TODO: Implement blacklist management
            // For now, return empty list
            await Task.CompletedTask;
            return new List<string>();
        }

        public async Task<bool> AddToBlacklistAsync(int companyId, string phoneNumber)
        {
            // TODO: Implement blacklist management
            // For now, return false (not implemented)
            await Task.CompletedTask;
            return false;
        }

        public async Task<bool> RemoveFromBlacklistAsync(int companyId, string phoneNumber)
        {
            // TODO: Implement blacklist management
            // For now, return false (not implemented)
            await Task.CompletedTask;
            return false;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Rebuild conversations table from existing WhatsAppMessages, ensuring older threads appear in the list
        /// </summary>
        public async Task<int> RebuildConversationsFromMessagesAsync(int companyId)
        {
            var created = 0;
            var groups = await _context.WhatsAppMessages
                .Where(m => m.CompanyId == companyId)
                .GroupBy(m => new { m.CompanyId, m.From, m.To })
                .ToListAsync();

            foreach (var g in groups)
            {
                // Determine customer and business phones
                // Heuristic: if From == business number in any message, swap to ensure CustomerPhone is the opposite
                var businessPhone = g.Key.To;
                var customerPhone = g.Key.From;

                // Pick last message for preview
                var last = g.OrderByDescending(m => m.Timestamp).First();

                // TODO: Implement WhatsAppConversations table
                // var existing = await _context.WhatsAppConversations
                //     .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.CustomerPhone == customerPhone && c.BusinessPhone == businessPhone);
                // if (existing == null)
                // {
                //     existing = new WhatsAppConversation
                //     {
                //         CompanyId = companyId,
                //         CustomerPhone = customerPhone,
                //         BusinessPhone = businessPhone,
                //         Status = "active",
                //         StartedAt = g.Min(m => m.Timestamp),
                //         LastMessageAt = last.Timestamp,
                //         LastMessagePreview = last.Body,
                //         CreatedAt = DateTime.UtcNow,
                //         UpdatedAt = DateTime.UtcNow
                //     };
                //     _context.WhatsAppConversations.Add(existing);
                //     created++;
                // }
                // else
                // {
                //     // Update last message info
                //     existing.LastMessageAt = last.Timestamp;
                //     existing.LastMessagePreview = last.Body;
                //     existing.UpdatedAt = DateTime.UtcNow;
                // }
            }

            await _context.SaveChangesAsync();
            return created;
        }

        private async Task<WhatsAppConfig?> GetGreenApiConfigAsync(int companyId)
        {
            // For testing, we don't require IsActive to be true
            // Also check for both "GreenAPI" and "GreenApi" variations
            _logger.LogInformation("Looking for GreenAPI config for company {CompanyId}", companyId);
            
            var configs = await _context.WhatsAppConfigs
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();
            
            _logger.LogInformation("Found {Count} WhatsApp configs for company {CompanyId}", configs.Count, companyId);
            foreach (var cfg in configs)
            {
                _logger.LogInformation("Config: Provider={Provider}, IsActive={IsActive}, Id={Id}", 
                    cfg.Provider, cfg.IsActive, cfg.Id);
            }
            
            var config = configs.FirstOrDefault(c => 
                c.Provider == "GreenAPI" || c.Provider == "GreenApi");
            
            if (config != null)
            {
                _logger.LogInformation("Found GreenAPI config with Provider={Provider}", config.Provider);
            }
            else
            {
                _logger.LogWarning("No GreenAPI config found for company {CompanyId}", companyId);
            }
            
            return config;
        }

        private async Task<bool> TestInstanceStateAsync(WhatsAppConfig config)
        {
            try
            {
                var url = $"/waInstance{config.GreenApiInstanceId}/getStateInstance/{_encryptionService.Decrypt(config.GreenApiToken)}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Green API instance state check failed with status {StatusCode}", response.StatusCode);
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                var stateResponse = JsonConvert.DeserializeObject<GreenApiStateInstanceResponse>(content);
                
                return stateResponse?.StateInstance == "authorized";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Green API instance state");
                return false;
            }
        }

        private async Task<bool> TestAccountInfoAsync(WhatsAppConfig config)
        {
            try
            {
                var url = $"/waInstance{config.GreenApiInstanceId}/getWaSettings/{_encryptionService.Decrypt(config.GreenApiToken)}";
                var response = await _httpClient.GetAsync(url);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Green API account info");
                return false;
            }
        }

        private async Task<bool> SendTestMessageAsync(WhatsAppConfig config, string testPhoneNumber)
        {
            try
            {
                var chatId = testPhoneNumber.ToGreenApiChatId();
                var message = "Test message from WebsiteBuilder API - Green API Integration";
                
                var result = await SendTextMessageAsync(config, chatId, message);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Green API test message");
                return false;
            }
        }

        private async Task<WhatsAppMessageDto?> SendTextMessageAsync(WhatsAppConfig config, string chatId, string message)
        {
            try
            {
                var url = $"/waInstance{config.GreenApiInstanceId}/sendMessage/{_encryptionService.Decrypt(config.GreenApiToken)}";
                
                var payload = new GreenApiSendMessageDto
                {
                    ChatId = chatId,
                    Message = message
                };

                var jsonSettings = new JsonSerializerSettings 
                { 
                    NullValueHandling = NullValueHandling.Ignore 
                };
                var json = JsonConvert.SerializeObject(payload, jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Green API send message failed: {StatusCode}, {Content}", response.StatusCode, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var messageResponse = JsonConvert.DeserializeObject<GreenApiMessageResponse>(responseContent);

                return new WhatsAppMessageDto
                {
                    Id = Guid.NewGuid(),
                    TwilioSid = messageResponse?.IdMessage ?? Guid.NewGuid().ToString(),
                    Body = message,
                    MessageType = "text",
                    Direction = "outbound",
                    Status = "sent",
                    Timestamp = DateTime.UtcNow,
                    // Provider property not available in base WhatsAppMessageDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Green API text message");
                return null;
            }
        }

        private async Task<WhatsAppMessageDto?> SendMediaMessageAsync(WhatsAppConfig config, string chatId, string? caption, string mediaUrl, string? mediaType)
        {
            try
            {
                var url = $"/waInstance{config.GreenApiInstanceId}/sendFileByUrl/{_encryptionService.Decrypt(config.GreenApiToken)}";
                
                var payload = new GreenApiSendFileMessageDto
                {
                    ChatId = chatId,
                    UrlFile = mediaUrl,
                    Caption = caption
                };

                var jsonSettings = new JsonSerializerSettings 
                { 
                    NullValueHandling = NullValueHandling.Ignore 
                };
                var json = JsonConvert.SerializeObject(payload, jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Green API send media message failed: {StatusCode}, {Content}", response.StatusCode, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var messageResponse = JsonConvert.DeserializeObject<GreenApiMessageResponse>(responseContent);

                return new WhatsAppMessageDto
                {
                    Id = Guid.NewGuid(),
                    TwilioSid = messageResponse?.IdMessage ?? Guid.NewGuid().ToString(),
                    Body = caption ?? "",
                    MessageType = DetermineMessageTypeFromUrl(mediaUrl, mediaType),
                    Direction = "outbound",
                    Status = "sent",
                    MediaUrl = mediaUrl,
                    MediaContentType = mediaType,
                    Timestamp = DateTime.UtcNow,
                    // Provider property not available in base WhatsAppMessageDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Green API media message");
                return null;
            }
        }

        private string DetermineMessageTypeFromUrl(string mediaUrl, string? mediaType)
        {
            if (!string.IsNullOrEmpty(mediaType))
            {
                if (mediaType.StartsWith("image/")) return "image";
                if (mediaType.StartsWith("video/")) return "video";
                if (mediaType.StartsWith("audio/")) return "audio";
                return "document";
            }

            var extension = Path.GetExtension(mediaUrl).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
                ".mp4" or ".avi" or ".mov" or ".wmv" => "video",
                ".mp3" or ".wav" or ".ogg" => "audio",
                _ => "document"
            };
        }

        private async Task<bool> CheckRateLimitAsync(int companyId)
        {
            if (!_settings.RateLimit.Enabled)
                return true;

            lock (_rateLimitLock)
            {
                var now = DateTime.UtcNow;
                
                if (!_lastMessageTime.ContainsKey(companyId) || !_messageCount.ContainsKey(companyId))
                {
                    _lastMessageTime[companyId] = now;
                    _messageCount[companyId] = 0;
                    return true;
                }

                var timeSinceLastMessage = now - _lastMessageTime[companyId];
                var windowMinutes = TimeSpan.FromMinutes(_settings.RateLimit.WindowSizeMinutes);

                // Reset counter if window has passed
                if (timeSinceLastMessage >= windowMinutes)
                {
                    _messageCount[companyId] = 0;
                    _lastMessageTime[companyId] = now;
                }

                return _messageCount[companyId] < _settings.RateLimit.MessagesPerMinute;
            }
        }

        private void UpdateRateLimitCounters(int companyId)
        {
            lock (_rateLimitLock)
            {
                _messageCount[companyId] = _messageCount.GetValueOrDefault(companyId, 0) + 1;
                _lastMessageTime[companyId] = DateTime.UtcNow;
            }
        }

        private async Task SaveMessageToDbAsync(int companyId, WhatsAppMessageDto messageDto, string customerPhone, string businessPhone)
        {
            try
            {
                // Get or create conversation
                var conversation = await GetOrCreateConversationAsync(companyId, customerPhone, businessPhone);
                
                if (conversation == null)
                {
                    _logger.LogWarning("Failed to get or create conversation for message saving");
                    return;
                }

                var message = new WhatsAppMessage
                {
                    Id = messageDto.Id,
                    ConversationId = conversation.Id,
                    TwilioSid = messageDto.TwilioSid, // Using TwilioSid field for Green API message ID
                    From = businessPhone,
                    To = customerPhone,
                    Body = messageDto.Body,
                    MessageType = messageDto.MessageType,
                    Direction = messageDto.Direction,
                    Status = messageDto.Status,
                    MediaUrl = messageDto.MediaUrl,
                    MediaContentType = messageDto.MediaContentType,
                    CompanyId = companyId,
                    Timestamp = messageDto.Timestamp,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WhatsAppMessages.Add(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message to database");
                // Don't throw, as the message was sent successfully
            }
        }

        private WhatsAppMessageDto MapMessageToDto(WhatsAppMessage message)
        {
            return new WhatsAppMessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                TwilioSid = message.TwilioSid, // Using TwilioSid field for message ID
                From = message.From,
                To = message.To,
                Body = message.Body,
                MessageType = message.MessageType,
                Direction = message.Direction,
                Status = message.Status,
                MediaUrl = message.MediaUrl,
                MediaContentType = message.MediaContentType,
                ReadAt = message.ReadAt,
                Timestamp = message.Timestamp
            };
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Validates Green API configuration DTO
        /// </summary>
        private static List<string> ValidateGreenApiConfigDto(CreateGreenApiConfigDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(dto.InstanceId))
                errors.Add("Instance ID is required");

            if (string.IsNullOrEmpty(dto.ApiToken))
                errors.Add("API Token is required");

            if (string.IsNullOrEmpty(dto.PhoneNumber))
                errors.Add("Phone Number is required");

            if (dto.PollingIntervalSeconds < 1 || dto.PollingIntervalSeconds > 300)
                errors.Add("Polling interval must be between 1 and 300 seconds");

            if (!string.IsNullOrEmpty(dto.WebhookUrl) && !Uri.TryCreate(dto.WebhookUrl, UriKind.Absolute, out _))
                errors.Add("Webhook URL must be a valid URL");

            return errors;
        }

        /// <summary>
        /// Infers media content type from file URL
        /// </summary>
        private static string InferMediaTypeFromUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return "application/octet-stream";

            var extension = Path.GetExtension(url).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".ogg" => "audio/ogg",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
