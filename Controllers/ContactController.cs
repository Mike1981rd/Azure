using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Contact;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;
using System.Net;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ApplicationDbContext context, EmailService emailService, ILogger<ContactController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/contact/company/{companyId}/messages
        [HttpGet("company/{companyId}/messages")]
        public async Task<ActionResult<ApiResponse<List<ContactMessageDto>>>> GetContactMessages(
            int companyId,
            [FromQuery] ContactMessageFilterDto filter)
        {
            try
            {
                var query = _context.ContactMessages
                    .Where(c => c.CompanyId == companyId)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(c => c.Status == filter.Status);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(filter.SearchTerm) ||
                        c.Email.Contains(filter.SearchTerm) ||
                        c.Message.Contains(filter.SearchTerm));
                }

                // Apply pagination
                var totalCount = await query.CountAsync();
                var messages = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new ContactMessageDto
                    {
                        Id = c.Id,
                        CompanyId = c.CompanyId,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Message = c.Message,
                        Status = c.Status,
                        IsNotificationSent = c.IsNotificationSent,
                        CreatedAt = c.CreatedAt,
                        ReadAt = c.ReadAt,
                        ArchivedAt = c.ArchivedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<ContactMessageDto>>.SuccessResponse(
                    messages,
                    "Contact messages retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact messages for company {CompanyId}", companyId);
                return StatusCode(500, ApiResponse<List<ContactMessageDto>>.ErrorResponse(
                    "An error occurred while retrieving contact messages",
                    500
                ));
            }
        }

        // GET: api/contact/company/{companyId}/unread-count
        [HttpGet("company/{companyId}/unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(int companyId)
        {
            try
            {
                var count = await _context.ContactMessages
                    .Where(c => c.CompanyId == companyId && c.Status == "unread")
                    .CountAsync();

                return Ok(ApiResponse<int>.SuccessResponse(
                    count,
                    "Unread count retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count for company {CompanyId}", companyId);
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    "An error occurred while retrieving unread count",
                    500
                ));
            }
        }

        // POST: api/contact/company/{companyId}/submit
        [HttpPost("company/{companyId}/submit")]
        public async Task<ActionResult<ApiResponse<ContactMessageDto>>> SubmitContactMessage(
            int companyId,
            [FromBody] CreateContactMessageDto createDto)
        {
            try
            {
                // Get client IP and User Agent
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Create contact message
                var contactMessage = new ContactMessage
                {
                    CompanyId = companyId,
                    Name = createDto.Name,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    Message = createDto.Message,
                    Status = "unread",
                    IsNotificationSent = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync();

                // Send notifications asynchronously
                _ = Task.Run(async () => await SendNotificationsAsync(contactMessage));

                var responseDto = new ContactMessageDto
                {
                    Id = contactMessage.Id,
                    CompanyId = contactMessage.CompanyId,
                    Name = contactMessage.Name,
                    Email = contactMessage.Email,
                    Phone = contactMessage.Phone,
                    Message = contactMessage.Message,
                    Status = contactMessage.Status,
                    IsNotificationSent = contactMessage.IsNotificationSent,
                    CreatedAt = contactMessage.CreatedAt,
                    ReadAt = contactMessage.ReadAt,
                    ArchivedAt = contactMessage.ArchivedAt
                };

                return Ok(ApiResponse<ContactMessageDto>.SuccessResponse(
                    responseDto,
                    "Contact message submitted successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact message for company {CompanyId}", companyId);
                return StatusCode(500, ApiResponse<ContactMessageDto>.ErrorResponse(
                    "An error occurred while submitting the contact message",
                    500
                ));
            }
        }

        // PUT: api/contact/company/{companyId}/messages/{messageId}/status
        [HttpPut("company/{companyId}/messages/{messageId}/status")]
        public async Task<ActionResult<ApiResponse<ContactMessageDto>>> UpdateMessageStatus(
            int companyId,
            int messageId,
            [FromBody] UpdateContactMessageStatusDto updateDto)
        {
            try
            {
                var message = await _context.ContactMessages
                    .FirstOrDefaultAsync(c => c.Id == messageId && c.CompanyId == companyId);

                if (message == null)
                {
                    return NotFound(ApiResponse<ContactMessageDto>.ErrorResponse(
                        "Contact message not found",
                        404
                    ));
                }

                message.Status = updateDto.Status;
                
                if (updateDto.Status == "read" && message.ReadAt == null)
                {
                    message.ReadAt = DateTime.UtcNow;
                }
                
                if (updateDto.Status == "archived" && message.ArchivedAt == null)
                {
                    message.ArchivedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var responseDto = new ContactMessageDto
                {
                    Id = message.Id,
                    CompanyId = message.CompanyId,
                    Name = message.Name,
                    Email = message.Email,
                    Phone = message.Phone,
                    Message = message.Message,
                    Status = message.Status,
                    IsNotificationSent = message.IsNotificationSent,
                    CreatedAt = message.CreatedAt,
                    ReadAt = message.ReadAt,
                    ArchivedAt = message.ArchivedAt
                };

                return Ok(ApiResponse<ContactMessageDto>.SuccessResponse(
                    responseDto,
                    "Message status updated successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message status for message {MessageId}", messageId);
                return StatusCode(500, ApiResponse<ContactMessageDto>.ErrorResponse(
                    "An error occurred while updating the message status",
                    500
                ));
            }
        }

        // GET: api/contact/company/{companyId}/notification-settings
        [HttpGet("company/{companyId}/notification-settings")]
        public async Task<ActionResult<ApiResponse<ContactNotificationSettingsDto>>> GetNotificationSettings(int companyId)
        {
            try
            {
                var settings = await _context.ContactNotificationSettings
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                if (settings == null)
                {
                    // Create default settings
                    settings = new ContactNotificationSettings
                    {
                        CompanyId = companyId,
                        EmailNotificationsEnabled = true,
                        ToastNotificationsEnabled = true,
                        DashboardNotificationsEnabled = true,
                        PlaySoundOnNewMessage = false,
                        NotificationEmailAddress = "",
                        EmailSubjectTemplate = "New Contact Message from {name}",
                        ToastSuccessMessage = "Message sent successfully!",
                        ToastErrorMessage = "Error sending message. Please try again."
                    };

                    _context.ContactNotificationSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                var settingsDto = new ContactNotificationSettingsDto
                {
                    Id = settings.Id,
                    CompanyId = settings.CompanyId,
                    EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
                    ToastNotificationsEnabled = settings.ToastNotificationsEnabled,
                    DashboardNotificationsEnabled = settings.DashboardNotificationsEnabled,
                    PlaySoundOnNewMessage = settings.PlaySoundOnNewMessage,
                    NotificationEmailAddress = settings.NotificationEmailAddress,
                    EmailSubjectTemplate = settings.EmailSubjectTemplate,
                    ToastSuccessMessage = settings.ToastSuccessMessage,
                    ToastErrorMessage = settings.ToastErrorMessage
                };

                return Ok(ApiResponse<ContactNotificationSettingsDto>.SuccessResponse(
                    settingsDto,
                    "Notification settings retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification settings for company {CompanyId}", companyId);
                return StatusCode(500, ApiResponse<ContactNotificationSettingsDto>.ErrorResponse(
                    "An error occurred while retrieving notification settings",
                    500
                ));
            }
        }

        // PUT: api/contact/company/{companyId}/notification-settings
        [HttpPut("company/{companyId}/notification-settings")]
        public async Task<ActionResult<ApiResponse<ContactNotificationSettingsDto>>> UpdateNotificationSettings(
            int companyId,
            [FromBody] UpdateContactNotificationSettingsDto updateDto)
        {
            try
            {
                var settings = await _context.ContactNotificationSettings
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId);

                if (settings == null)
                {
                    settings = new ContactNotificationSettings
                    {
                        CompanyId = companyId
                    };
                    _context.ContactNotificationSettings.Add(settings);
                }

                settings.EmailNotificationsEnabled = updateDto.EmailNotificationsEnabled;
                settings.ToastNotificationsEnabled = updateDto.ToastNotificationsEnabled;
                settings.DashboardNotificationsEnabled = updateDto.DashboardNotificationsEnabled;
                settings.PlaySoundOnNewMessage = updateDto.PlaySoundOnNewMessage;
                settings.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(updateDto.NotificationEmailAddress))
                {
                    settings.NotificationEmailAddress = updateDto.NotificationEmailAddress;
                }

                if (!string.IsNullOrEmpty(updateDto.EmailSubjectTemplate))
                {
                    settings.EmailSubjectTemplate = updateDto.EmailSubjectTemplate;
                }

                if (!string.IsNullOrEmpty(updateDto.ToastSuccessMessage))
                {
                    settings.ToastSuccessMessage = updateDto.ToastSuccessMessage;
                }

                if (!string.IsNullOrEmpty(updateDto.ToastErrorMessage))
                {
                    settings.ToastErrorMessage = updateDto.ToastErrorMessage;
                }

                await _context.SaveChangesAsync();

                var settingsDto = new ContactNotificationSettingsDto
                {
                    Id = settings.Id,
                    CompanyId = settings.CompanyId,
                    EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
                    ToastNotificationsEnabled = settings.ToastNotificationsEnabled,
                    DashboardNotificationsEnabled = settings.DashboardNotificationsEnabled,
                    PlaySoundOnNewMessage = settings.PlaySoundOnNewMessage,
                    NotificationEmailAddress = settings.NotificationEmailAddress,
                    EmailSubjectTemplate = settings.EmailSubjectTemplate,
                    ToastSuccessMessage = settings.ToastSuccessMessage,
                    ToastErrorMessage = settings.ToastErrorMessage
                };

                return Ok(ApiResponse<ContactNotificationSettingsDto>.SuccessResponse(
                    settingsDto,
                    "Notification settings updated successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings for company {CompanyId}", companyId);
                return StatusCode(500, ApiResponse<ContactNotificationSettingsDto>.ErrorResponse(
                    "An error occurred while updating notification settings",
                    500
                ));
            }
        }

        private async Task SendNotificationsAsync(ContactMessage contactMessage)
        {
            try
            {
                var settings = await _context.ContactNotificationSettings
                    .FirstOrDefaultAsync(s => s.CompanyId == contactMessage.CompanyId);

                if (settings?.EmailNotificationsEnabled == true && !string.IsNullOrEmpty(settings.NotificationEmailAddress))
                {
                    var emailSubject = settings.EmailSubjectTemplate.Replace("{name}", contactMessage.Name);
                    var emailBody = $@"
                        <h2>New Contact Message</h2>
                        <p><strong>Name:</strong> {contactMessage.Name}</p>
                        <p><strong>Email:</strong> {contactMessage.Email}</p>
                        {(!string.IsNullOrEmpty(contactMessage.Phone) ? $"<p><strong>Phone:</strong> {contactMessage.Phone}</p>" : "")}
                        <p><strong>Message:</strong></p>
                        <p>{contactMessage.Message.Replace("\n", "<br>")}</p>
                        <hr>
                        <p><small>Received at: {contactMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</small></p>
                    ";

                    await _emailService.SendEmailAsync(
                        settings.NotificationEmailAddress,
                        emailSubject,
                        emailBody
                    );

                    // Mark notification as sent
                    contactMessage.IsNotificationSent = true;
                    await _context.SaveChangesAsync();
                }

                // Send a copy to the customer (receipt of their message)
                if (!string.IsNullOrWhiteSpace(contactMessage.Email))
                {
                    var customerBody = $@"
                        <h2>Hemos recibido tu mensaje</h2>
                        <p><strong>Nombre:</strong> {contactMessage.Name}</p>
                        <p><strong>Tu mensaje:</strong></p>
                        <p>{contactMessage.Message.Replace("\n", "<br>")}</p>
                        <hr>
                        <p><small>Enviado: {contactMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</small></p>
                    ";
                    await _emailService.SendEmailAsync(contactMessage.Email, "Copia de tu mensaje", customerBody);
                }

                // App notification (bell)
                try
                {
                    var notifier = HttpContext.RequestServices.GetRequiredService<INotificationService>();
                    await notifier.CreateAsync(contactMessage.CompanyId,
                        type: "contact_message",
                        title: $"Nuevo mensaje de {contactMessage.Name}",
                        message: contactMessage.Message.Length > 120 ? contactMessage.Message.Substring(0,120)+"..." : contactMessage.Message,
                        data: new { contactMessage.Id, contactMessage.Email },
                        relatedType: "contact_message",
                        relatedId: contactMessage.Id.ToString());
                }
                catch { }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for contact message {MessageId}", contactMessage.Id);
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out _))
            {
                return ipAddress.Length > 45 ? ipAddress.Substring(0, 45) : ipAddress;
            }
            return "unknown";
        }
    }
}
