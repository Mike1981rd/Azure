using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// DTO for WhatsApp message display
    /// </summary>
    public class WhatsAppMessageDto
    {
        public Guid Id { get; set; }
        public string TwilioSid { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string MessageType { get; set; } = "text";
        public string? MediaUrl { get; set; }
        public string? MediaContentType { get; set; }
        public string Direction { get; set; } = "inbound";
        public string Status { get; set; } = "received";
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid ConversationId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? RepliedByUserId { get; set; }
        public string? RepliedByUserName { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO for sending WhatsApp messages
    /// </summary>
    public class SendWhatsAppMessageDto
    {
        [Required]
        [Phone]
        [StringLength(20, MinimumLength = 10)]
        public string To { get; set; } = string.Empty;

        [StringLength(4096)]
        public string? Body { get; set; }

        [Url]
        [StringLength(500)]
        public string? MediaUrl { get; set; }

        public string MessageType { get; set; } = "text";

        public string? TemplateId { get; set; }

        public Dictionary<string, string>? TemplateParameters { get; set; }
    }

    /// <summary>
    /// DTO for bulk sending messages
    /// </summary>
    public class BulkSendWhatsAppMessageDto
    {
        [Required]
        [MinLength(1)]
        public List<string> To { get; set; } = new();

        [Required]
        [StringLength(4096)]
        public string Body { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        public string? MediaUrl { get; set; }

        public string MessageType { get; set; } = "text";

        public string? TemplateId { get; set; }

        public Dictionary<string, string>? TemplateParameters { get; set; }

        public bool SendImmediately { get; set; } = true;

        public DateTime? ScheduledAt { get; set; }
    }

    // MessageTemplateDto and CreateMessageTemplateDto are defined in WhatsAppConversationDto.cs

    /// <summary>
    /// DTO for Twilio webhook incoming messages
    /// </summary>
    public class TwilioWebhookDto
    {
        public string MessageSid { get; set; } = string.Empty;
        public string AccountSid { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? MediaUrl0 { get; set; }
        public string? MediaContentType0 { get; set; }
        public string? ProfileName { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonPayload { get; set; }
        public int NumMedia { get; set; }
        public int NumSegments { get; set; }
    }

    /// <summary>
    /// DTO for message status updates from Twilio
    /// </summary>
    public class TwilioStatusWebhookDto
    {
        public string MessageSid { get; set; } = string.Empty;
        public string MessageStatus { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}