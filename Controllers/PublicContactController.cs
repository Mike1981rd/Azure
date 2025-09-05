using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;
using System.Net;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Public endpoint for contact form submissions (no auth required)
    /// </summary>
    [ApiController]
    [Route("api/public/contact")]
    public class PublicContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublicContactController> _logger;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public PublicContactController(
            ApplicationDbContext context,
            ILogger<PublicContactController> logger,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Submit a contact message from public website
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitContactMessage(
            [FromBody] PublicContactMessageDto dto)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.Name) || 
                    string.IsNullOrWhiteSpace(dto.Email) || 
                    string.IsNullOrWhiteSpace(dto.Message))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("All fields are required"));
                }

                // Validate email format
                if (!IsValidEmail(dto.Email))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid email format"));
                }

                // Get client IP and User Agent
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Validate that company exists
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
                if (!companyExists)
                {
                    // Try to get the first company as fallback
                    var firstCompany = await _context.Companies.FirstOrDefaultAsync();
                    if (firstCompany == null)
                    {
                        _logger.LogWarning("No companies found in database");
                        return BadRequest(ApiResponse<object>.ErrorResponse("Configuration error. Please contact support."));
                    }
                    dto.CompanyId = firstCompany.Id;
                    _logger.LogWarning("Invalid company ID {RequestedId}, using default {DefaultId}", dto.CompanyId, firstCompany.Id);
                }

                // Create contact message
                var contactMessage = new ContactMessage
                {
                    CompanyId = dto.CompanyId,
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    Phone = dto.Phone?.Trim(),
                    Subject = dto.Subject?.Trim() ?? "Website Contact Form",
                    Message = dto.Message.Trim(),
                    Source = "website",
                    Status = "unread",
                    IsNotificationSent = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    _context.ContactMessages.Add(contactMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error while saving contact message");
                    // Log inner exception for more details
                    if (dbEx.InnerException != null)
                    {
                        _logger.LogError(dbEx.InnerException, "Inner exception details");
                    }
                    return StatusCode(500, ApiResponse<object>.ErrorResponse(
                        "Unable to save your message at this time. Please try again later."
                    ));
                }

                _logger.LogInformation("Contact message submitted from website for company {CompanyId}", dto.CompanyId);

                // Emails
                try
                {
                    // Send copy to customer
                    var customerBody = $@"<h2>Hemos recibido tu mensaje</h2><p><strong>Nombre:</strong> {contactMessage.Name}</p><p><strong>Tu mensaje:</strong></p><p>{contactMessage.Message.Replace("\n", "<br>")}</p>";
                    await _emailService.SendEmailAsync(contactMessage.Email, "Copia de tu mensaje", customerBody);

                    // Send to company contact email if available
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == dto.CompanyId);
                    var adminEmail = company?.ContactEmail ?? company?.SenderEmail;
                    if (!string.IsNullOrWhiteSpace(adminEmail))
                    {
                        var adminBody = $@"<h2>Nuevo mensaje de contacto</h2><p><strong>Nombre:</strong> {contactMessage.Name}</p><p><strong>Email:</strong> {contactMessage.Email}</p>{(!string.IsNullOrEmpty(contactMessage.Phone) ? $"<p><strong>Phone:</strong> {contactMessage.Phone}</p>" : "")}<p><strong>Mensaje:</strong></p><p>{contactMessage.Message.Replace("\n", "<br>")}</p>";
                        await _emailService.SendEmailAsync(adminEmail, "Nuevo mensaje de contacto", adminBody);
                    }
                }
                catch { }

                // Bell notification
                try
                {
                    await _notificationService.CreateAsync(dto.CompanyId, "contact_message", $"Nuevo mensaje de {contactMessage.Name}", contactMessage.Message, new { contactMessage.Id, contactMessage.Email }, "contact_message", contactMessage.Id.ToString());
                }
                catch { }

                // Return success without exposing internal details
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { id = contactMessage.Id },
                    "Message sent successfully. We'll get back to you soon!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact message");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while sending your message. Please try again later."
                ));
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GetClientIpAddress()
        {
            // Check for forwarded IP (when behind proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip.Length > 45 ? ip.Substring(0, 45) : ip;
            }

            // Check X-Real-IP header
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp.Length > 45 ? realIp.Substring(0, 45) : realIp;

            // Fall back to connection IP
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress.Length > 45 ? ipAddress.Substring(0, 45) : ipAddress;
            }

            return "unknown";
        }
    }

    /// <summary>
    /// DTO for public contact form submissions
    /// </summary>
    public class PublicContactMessageDto
    {
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
