using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ApplicationDbContext context,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            await SendEmailAsync(to, subject, htmlBody, null);
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string from)
        {
            await SendEmailAsync(to, subject, htmlBody, from, null);
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? from, IEnumerable<EmailAttachment>? attachments)
        {
            try
            {
                // Resolve provider from DB (single-tenant companyId = 1 for now)
                var settings = await _context.Set<EmailProviderSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.CompanyId == 1);

                var provider = settings?.Provider?.Trim() ?? "Postmark";
                if (!string.IsNullOrEmpty(provider) && provider.Equals("Postmark", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(settings?.ApiKey))
                {
                    await SendViaPostmarkAsync(_encryptionService.Decrypt(settings!.ApiKey!), to, subject, htmlBody, from ?? settings.FromEmail, settings.FromName, attachments);
                    return;
                }

                // Fallback: log only
                _logger.LogInformation("[Email Fallback] To={To} Subject={Subject} From={From}", to, subject, from ?? settings?.FromEmail ?? "(default)");
                _logger.LogDebug("Body: {Body}", htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        private async Task SendViaPostmarkAsync(string apiToken, string to, string subject, string htmlBody, string? fromEmail, string? fromName, IEnumerable<EmailAttachment>? attachments)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.postmarkapp.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Postmark-Server-Token", apiToken);

            var from = !string.IsNullOrWhiteSpace(fromName) && !string.IsNullOrWhiteSpace(fromEmail)
                ? $"{fromName} <{fromEmail}>" : (fromEmail ?? _configuration["Email:DefaultFrom"] ?? "no-reply@localhost");

            var payload = new Dictionary<string, object?>
            {
                ["From"] = from,
                ["To"] = to,
                ["Subject"] = subject,
                ["HtmlBody"] = htmlBody,
                ["MessageStream"] = "outbound"
            };

            if (attachments != null)
            {
                payload["Attachments"] = attachments.Select(a => new Dictionary<string, object>
                {
                    ["Name"] = a.FileName,
                    ["Content"] = Convert.ToBase64String(a.Content),
                    ["ContentType"] = a.ContentType
                }).ToList();
            }

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("email", content);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogError("Postmark send failed: {Status} {Body}", (int)resp.StatusCode, body);
                throw new InvalidOperationException($"Postmark send failed: {(int)resp.StatusCode}");
            }
            _logger.LogInformation("Email sent via Postmark to {To} - {Subject}", to, subject);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Welcome to Our Platform";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome {firstName}!</h2>
                    <p>Thank you for creating an account with us.</p>
                    <p>You can now manage your reservations and view your booking history.</p>
                    <p>If you have any questions, please don't hesitate to contact us.</p>
                    <br>
                    <p>Best regards,<br>The Team</p>
                </body>
                </html>";
                
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var resetUrl = $"{_configuration["WebsiteUrl"]}/reset-password?token={resetToken}";
            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password.</p>
                    <p>Please click the link below to reset your password:</p>
                    <p><a href='{resetUrl}'>Reset Password</a></p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>This link will expire in 24 hours.</p>
                    <br>
                    <p>Best regards,<br>The Team</p>
                </body>
                </html>";
                
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendReservationConfirmationAsync(string email, int reservationId)
        {
            var subject = $"Reservation Confirmed - #{reservationId:D6}";
            var body = $@"
                <html>
                <body>
                    <h2>Reservation Confirmed!</h2>
                    <p>Your reservation #{reservationId:D6} has been confirmed.</p>
                    <p>You will receive more details shortly.</p>
                    <p>You can view your reservation details by logging into your account.</p>
                    <br>
                    <p>Thank you for choosing us!</p>
                    <p>Best regards,<br>The Team</p>
                </body>
                </html>";
                
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAccountCreatedEmailAsync(string email, string username, string temporaryPassword)
        {
            var loginUrl = $"{_configuration["WebsiteUrl"]}/login";
            var subject = "Your Account Has Been Created";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #22c55e;'>Your Account Has Been Created!</h2>
                        <p>We've created an account for you to manage your reservation.</p>
                        
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h3 style='margin-top: 0;'>Your Login Credentials:</h3>
                            <p><strong>Email/Username:</strong> {email}</p>
                            <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                        </div>
                        
                        <p style='color: #ff6b6b;'><strong>Important:</strong> For security reasons, please change your password after your first login.</p>
                        
                        <div style='margin: 30px 0;'>
                            <a href='{loginUrl}' style='background-color: #22c55e; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Login to Your Account</a>
                        </div>
                        
                        <p>If you have any questions, please contact our support team.</p>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 12px; color: #666;'>
                            This email was sent because a reservation was made using your email address. 
                            If you didn't make this reservation, please contact us immediately.
                        </p>
                    </div>
                </body>
                </html>";
                
            await SendEmailAsync(email, subject, body);
        }
    }
}
