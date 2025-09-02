using WebsiteBuilderAPI.DTOs.WhatsApp;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Common interface for WhatsApp services (Twilio, GREEN-API, etc.)
    /// This interface defines the core WhatsApp functionality that all providers must implement
    /// </summary>
    public interface IWhatsAppService
    {
        // Configuration Management
        Task<WhatsAppConfigDto?> GetConfigAsync(int companyId);
        Task<WhatsAppConfigDto> CreateConfigAsync(int companyId, CreateWhatsAppConfigDto dto);
        Task<WhatsAppConfigDto> UpdateConfigAsync(int companyId, UpdateWhatsAppConfigDto dto);
        Task<bool> DeleteConfigAsync(int companyId);
        Task<WhatsAppConfigTestResultDto> TestConfigAsync(int companyId, TestWhatsAppConfigDto dto);

        // Message Sending - Core functionality
        Task<WhatsAppMessageDto> SendMessageAsync(int companyId, SendWhatsAppMessageDto dto);
        Task<List<WhatsAppMessageDto>> SendBulkMessageAsync(int companyId, BulkSendWhatsAppMessageDto dto);
        Task<WhatsAppMessageDto> SendTemplateMessageAsync(int companyId, string to, string templateId, Dictionary<string, string>? parameters = null);

        // Message Management
        Task<WhatsAppMessageDto?> GetMessageAsync(int companyId, Guid messageId);
        Task<List<WhatsAppMessageDto>> GetMessagesAsync(int companyId, Guid conversationId, int page = 1, int pageSize = 50);
        Task<bool> MarkMessageAsReadAsync(int companyId, Guid messageId);
        Task<bool> MarkConversationAsReadAsync(int companyId, Guid conversationId);

        // Conversation Management
        Task<WhatsAppConversationListDto> GetConversationsAsync(int companyId, WhatsAppConversationFilterDto filter);
        Task<WhatsAppConversationDetailDto?> GetConversationDetailAsync(int companyId, Guid conversationId);
        Task<WhatsAppConversationDto?> GetOrCreateConversationAsync(int companyId, string customerPhone, string businessPhone);
        Task<WhatsAppConversationDto> UpdateConversationAsync(int companyId, Guid conversationId, UpdateWhatsAppConversationDto dto);
        Task<bool> CloseConversationAsync(int companyId, Guid conversationId);
        Task<bool> ArchiveConversationAsync(int companyId, Guid conversationId);

        // Webhook Processing - Provider specific but unified interface
        Task<bool> ProcessIncomingMessageAsync(object webhookData);
        Task<bool> ProcessMessageStatusAsync(object statusData);
        Task<bool> ValidateWebhookSignatureAsync(string signature, string body, string? token = null);

        // Template Management
        Task<List<MessageTemplateDto>> GetMessageTemplatesAsync(int companyId);
        Task<MessageTemplateDto> CreateMessageTemplateAsync(int companyId, CreateMessageTemplateDto dto);
        Task<MessageTemplateDto> UpdateMessageTemplateAsync(int companyId, string templateId, CreateMessageTemplateDto dto);
        Task<bool> DeleteMessageTemplateAsync(int companyId, string templateId);

        // Statistics and Analytics
        Task<ConversationStatsDto> GetConversationStatsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, object>> GetMessageAnalyticsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);

        // Utility Methods
        Task<bool> IsWithinBusinessHoursAsync(int companyId);
        Task<string> FormatPhoneNumberAsync(string phoneNumber);
        Task<bool> IsValidPhoneNumberAsync(string phoneNumber);
        Task<List<string>> GetBlacklistedNumbersAsync(int companyId);
        Task<bool> AddToBlacklistAsync(int companyId, string phoneNumber);
        Task<bool> RemoveFromBlacklistAsync(int companyId, string phoneNumber);

        // Provider identification
        string ProviderName { get; }
        bool IsConfigured(int companyId);
    }
}