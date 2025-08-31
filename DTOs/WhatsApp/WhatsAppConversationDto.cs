using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// DTO for WhatsApp conversation display
    /// </summary>
    public class WhatsAppConversationDto
    {
        public Guid Id { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string BusinessPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public string Priority { get; set; } = "normal";
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerEmail { get; set; }
        public int UnreadCount { get; set; }
        public int MessageCount { get; set; }
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessageSender { get; set; }
        public List<string>? Tags { get; set; }
        public string? Notes { get; set; }
        public CustomerProfileDto? CustomerProfile { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// DTO for conversation list with pagination
    /// </summary>
    public class WhatsAppConversationListDto
    {
        public List<WhatsAppConversationDto> Conversations { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for conversation details with messages
    /// </summary>
    public class WhatsAppConversationDetailDto
    {
        public WhatsAppConversationDto Conversation { get; set; } = new();
        public List<WhatsAppMessageDto> Messages { get; set; } = new();
        public int MessageCount { get; set; }
    }

    /// <summary>
    /// DTO for updating conversation properties
    /// </summary>
    public class UpdateWhatsAppConversationDto
    {
        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(10)]
        public string? Priority { get; set; }

        public int? AssignedUserId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// DTO for conversation filters
    /// </summary>
    public class WhatsAppConversationFilterDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? AssignedUserId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? StartedAfter { get; set; }
        public DateTime? StartedBefore { get; set; }
        public bool? HasUnreadMessages { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "LastMessageAt";
        public string? SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for customer profile from WhatsApp
    /// </summary>
    public class CustomerProfileDto
    {
        public string? Name { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? About { get; set; }
        public bool IsBusinessAccount { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for conversation statistics
    /// </summary>
    public class ConversationStatsDto
    {
        public int TotalConversations { get; set; }
        public int ActiveConversations { get; set; }
        public int UnreadConversations { get; set; }
        public int ClosedConversations { get; set; }
        public int ArchivedConversations { get; set; }
        public int TotalMessages { get; set; }
        public int MessagesThisMonth { get; set; }
        public double AverageResponseTime { get; set; }
        public Dictionary<string, int> MessagesByDay { get; set; } = new();
        public Dictionary<string, int> ConversationsByStatus { get; set; } = new();
        public object? Period { get; set; }
    }

    /// <summary>
    /// DTO for message templates (placeholder for future implementation)
    /// </summary>
    public class MessageTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Status { get; set; } = "approved";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating message templates (placeholder for future implementation)
    /// </summary>
    public class CreateMessageTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(10)]
        public string Language { get; set; } = "en";
    }
}