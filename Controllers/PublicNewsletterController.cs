using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.NewsletterSubscribers;
using WebsiteBuilderAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/public/newsletter")]
    public class PublicNewsletterController : ControllerBase
    {
        private readonly INewsletterSubscriberService _subscriberService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public PublicNewsletterController(INewsletterSubscriberService subscriberService, IEmailService emailService, INotificationService notificationService)
        {
            _subscriberService = subscriberService;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Public endpoint for newsletter subscription from website forms
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PublicSubscribeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data", details = ModelState });

                var subscriber = await _subscriberService.PublicSubscribeAsync(dto);

                // Emails: to subscriber and to admin (company contact)
                try
                {
                    await _emailService.SendEmailAsync(
                        subscriber.Email,
                        "Gracias por suscribirte",
                        $"<h2>¡Suscripción confirmada!</h2><p>Has sido suscrito al boletín de noticias.</p>");

                    // Admin email
                    var companyId = 1;
                    // Try to fetch a contact email from company
                    var db = HttpContext.RequestServices.GetRequiredService<WebsiteBuilderAPI.Data.ApplicationDbContext>();
                    var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
                    var adminEmail = company?.ContactEmail ?? company?.SenderEmail;
                    if (!string.IsNullOrWhiteSpace(adminEmail))
                    {
                        await _emailService.SendEmailAsync(adminEmail,
                            "Nueva suscripción",
                            $"<p>Se ha suscrito: {subscriber.Email}</p>");
                    }
                }
                catch { }

                // App notification (bell)
                try
                {
                    await _notificationService.CreateAsync(1, "subscription_new", "Nueva suscripción", $"Email: {subscriber.Email}", new { subscriber.Id, subscriber.Email }, "subscriber", subscriber.Id.ToString());
                }
                catch { }

                return Ok(new 
                { 
                    success = true, 
                    message = "Successfully subscribed to newsletter", 
                    subscriber = new { 
                        subscriber.Id, 
                        subscriber.Email, 
                        subscriber.FirstName,
                        subscriber.LastName
                    } 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while subscribing", details = ex.Message });
            }
        }

        /// <summary>
        /// Public endpoint for newsletter unsubscription
        /// </summary>
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data", details = ModelState });

                // Default to company ID 1 for single-tenant setup
                int companyId = 1;
                
                var result = await _subscriberService.PublicUnsubscribeAsync(dto.Email, companyId);
                
                if (!result)
                    return NotFound(new { error = "Email not found or already unsubscribed" });

                return Ok(new { success = true, message = "Successfully unsubscribed from newsletter" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while unsubscribing", details = ex.Message });
            }
        }

        /// <summary>
        /// Check subscription status for an email
        /// </summary>
        [HttpGet("status/{email}")]
        public async Task<IActionResult> CheckStatus(string email)
        {
            try
            {
                // Default to company ID 1 for single-tenant setup
                int companyId = 1;
                
                var subscribers = await _subscriberService.GetByEmailAsync(companyId, email);
                var activeSubscriber = subscribers.FirstOrDefault(s => s.IsActive);

                if (activeSubscriber == null)
                    return Ok(new { subscribed = false });

                return Ok(new 
                { 
                    subscribed = true, 
                    subscribedAt = activeSubscriber.CreatedAt,
                    language = activeSubscriber.Language 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
    }

    // DTO for unsubscribe
    public class UnsubscribeDto
    {
        public string Email { get; set; } = "";
        public string Reason { get; set; } = "User requested unsubscribe";
    }
}
