using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebsiteBuilderAPI.DTOs.WhatsApp
{
    /// <summary>
    /// DTO for Green API configuration
    /// </summary>
    public class GreenApiConfigDto
    {
        [Required]
        [StringLength(50)]
        public string InstanceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ApiToken { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? WebhookUrl { get; set; }

        public bool EnableWebhook { get; set; } = true;
        
        public bool AutoAcknowledgeMessages { get; set; } = true;
        
        public int PollingIntervalSeconds { get; set; } = 10;
    }

    /// <summary>
    /// DTO for creating Green API configuration
    /// </summary>
    public class CreateGreenApiConfigDto
    {
        [Required]
        [StringLength(50)]
        public string InstanceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ApiToken { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        public string? WebhookUrl { get; set; }

        public bool EnableWebhook { get; set; } = true;
        public bool AutoAcknowledgeMessages { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 10;
        public bool IsActive { get; set; } = false;
    }

    /// <summary>
    /// Green API send message request
    /// </summary>
    public class GreenApiSendMessageDto
    {
        [Required]
        [JsonPropertyName("chatId")]
        [JsonProperty("chatId")]
        public string ChatId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("message")]
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("quotedMessageId")]
        [JsonProperty("quotedMessageId")]
        public string? QuotedMessageId { get; set; }

        [JsonPropertyName("archiveChat")]
        [JsonProperty("archiveChat")]
        public bool? ArchiveChat { get; set; }

        [JsonPropertyName("linkPreview")]
        [JsonProperty("linkPreview")]
        public bool? LinkPreview { get; set; }
    }

    /// <summary>
    /// Green API send file message request
    /// </summary>
    public class GreenApiSendFileMessageDto
    {
        [Required]
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("urlFile")]
        public string UrlFile { get; set; } = string.Empty;

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("quotedMessageId")]
        public string? QuotedMessageId { get; set; }
    }

    /// <summary>
    /// Green API message response
    /// </summary>
    public class GreenApiMessageResponse
    {
        [JsonPropertyName("idMessage")]
        public string IdMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API webhook notification
    /// </summary>
    public class GreenApiWebhookNotification
    {
        [JsonPropertyName("typeWebhook")]
        public string TypeWebhook { get; set; } = string.Empty;

        [JsonPropertyName("instanceData")]
        public GreenApiInstanceData? InstanceData { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("idMessage")]
        public string? IdMessage { get; set; }

        [JsonPropertyName("senderData")]
        public GreenApiSenderData? SenderData { get; set; }

        [JsonPropertyName("messageData")]
        public GreenApiMessageData? MessageData { get; set; }

        [JsonPropertyName("statusData")]
        public GreenApiStatusData? StatusData { get; set; }
    }

    /// <summary>
    /// Green API instance data
    /// </summary>
    public class GreenApiInstanceData
    {
        [JsonPropertyName("idInstance")]
        public string IdInstance { get; set; } = string.Empty;

        [JsonPropertyName("wid")]
        public string Wid { get; set; } = string.Empty;

        [JsonPropertyName("typeInstance")]
        public string TypeInstance { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API sender data
    /// </summary>
    public class GreenApiSenderData
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; } = string.Empty;

        [JsonPropertyName("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonPropertyName("senderName")]
        public string? SenderName { get; set; }

        [JsonPropertyName("senderContactName")]
        public string? SenderContactName { get; set; }
    }

    /// <summary>
    /// Green API message data
    /// </summary>
    public class GreenApiMessageData
    {
        [JsonPropertyName("typeMessage")]
        public string TypeMessage { get; set; } = string.Empty;

        [JsonPropertyName("textMessageData")]
        public GreenApiTextMessageData? TextMessageData { get; set; }

        [JsonPropertyName("fileMessageData")]
        public GreenApiFileMessageData? FileMessageData { get; set; }

        [JsonPropertyName("locationMessageData")]
        public GreenApiLocationMessageData? LocationMessageData { get; set; }

        [JsonPropertyName("contactMessageData")]
        public GreenApiContactMessageData? ContactMessageData { get; set; }

        [JsonPropertyName("quotedMessage")]
        public GreenApiQuotedMessage? QuotedMessage { get; set; }
    }

    /// <summary>
    /// Green API text message data
    /// </summary>
    public class GreenApiTextMessageData
    {
        [JsonPropertyName("textMessage")]
        public string TextMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API file message data
    /// </summary>
    public class GreenApiFileMessageData
    {
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("jpegThumbnail")]
        public string? JpegThumbnail { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }

    /// <summary>
    /// Green API location message data
    /// </summary>
    public class GreenApiLocationMessageData
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("jpegThumbnail")]
        public string? JpegThumbnail { get; set; }
    }

    /// <summary>
    /// Green API contact message data
    /// </summary>
    public class GreenApiContactMessageData
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("vcard")]
        public string Vcard { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API quoted message
    /// </summary>
    public class GreenApiQuotedMessage
    {
        [JsonPropertyName("stanzaId")]
        public string StanzaId { get; set; } = string.Empty;

        [JsonPropertyName("participant")]
        public string? Participant { get; set; }

        [JsonPropertyName("typeMessage")]
        public string TypeMessage { get; set; } = string.Empty;

        [JsonPropertyName("textMessage")]
        public string? TextMessage { get; set; }
    }

    /// <summary>
    /// Green API status data
    /// </summary>
    public class GreenApiStatusData
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("sendByApi")]
        public bool? SendByApi { get; set; }
    }

    /// <summary>
    /// Green API error response
    /// </summary>
    public class GreenApiErrorResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Green API account info
    /// </summary>
    public class GreenApiAccountInfo
    {
        [JsonPropertyName("wid")]
        public string Wid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API state instance response
    /// </summary>
    public class GreenApiStateInstanceResponse
    {
        [JsonPropertyName("stateInstance")]
        public string StateInstance { get; set; } = string.Empty;
    }

    /// <summary>
    /// Green API notification
    /// </summary>
    public class GreenApiNotification
    {
        [JsonPropertyName("receiptId")]
        public int ReceiptId { get; set; }

        [JsonPropertyName("body")]
        public GreenApiWebhookNotification Body { get; set; } = new();
    }

    /// <summary>
    /// Green API receive notification response
    /// </summary>
    public class GreenApiReceiveNotificationResponse
    {
        [JsonPropertyName("result")]
        public bool Result { get; set; }
    }

    /// <summary>
    /// Extension methods for Green API DTOs
    /// </summary>
    public static class GreenApiExtensions
    {
        /// <summary>
        /// Converts phone number to Green API chat ID format
        /// </summary>
        public static string ToGreenApiChatId(this string phoneNumber)
        {
            // Remove all non-numeric characters except +
            var cleaned = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            
            // Add country code if not present
            if (!cleaned.StartsWith("1") && cleaned.Length == 10)
            {
                cleaned = "1" + cleaned; // Default to US
            }
            
            return cleaned + "@c.us";
        }

        /// <summary>
        /// Converts Green API chat ID to phone number
        /// </summary>
        public static string FromGreenApiChatId(this string chatId)
        {
            return "+" + chatId.Replace("@c.us", "").Replace("@g.us", "");
        }

        /// <summary>
        /// Converts Unix timestamp to DateTime
        /// </summary>
        public static DateTime FromUnixTimestamp(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        /// <summary>
        /// Converts DateTime to Unix timestamp
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Maps Green API message type to unified message type
        /// </summary>
        public static string MapMessageType(string greenApiType)
        {
            return greenApiType?.ToLowerInvariant() switch
            {
                "textmessage" => "text",
                "imagemessage" => "image",
                "videomessage" => "video",
                "audiomessage" => "audio",
                "documentmessage" => "document",
                "locationmessage" => "location",
                "contactmessage" => "contact",
                "stickermessage" => "sticker",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Validates Green API configuration
        /// </summary>
        public static List<string> ValidateConfig(this GreenApiConfigDto config)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(config.InstanceId))
                errors.Add("Instance ID is required");

            if (string.IsNullOrEmpty(config.ApiToken))
                errors.Add("API Token is required");

            if (string.IsNullOrEmpty(config.PhoneNumber))
                errors.Add("Phone Number is required");

            if (config.PollingIntervalSeconds < 1 || config.PollingIntervalSeconds > 300)
                errors.Add("Polling interval must be between 1 and 300 seconds");

            if (!string.IsNullOrEmpty(config.WebhookUrl) && !Uri.TryCreate(config.WebhookUrl, UriKind.Absolute, out _))
                errors.Add("Webhook URL must be a valid URL");

            return errors;
        }
    }
}