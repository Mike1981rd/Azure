using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebsiteBuilderAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            await SendEmailAsync(to, subject, htmlBody, null);
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string from)
        {
            try
            {
                // In production, this would use an actual email service like SendGrid, Mailgun, or SMTP
                // For now, we'll just log the email
                
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
                _logger.LogDebug("Email body: {Body}", htmlBody);
                
                // Simulate async operation
                await Task.Delay(100);
                
                // In production, you would implement actual email sending here:
                // Example with SendGrid:
                // var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
                // var fromAddress = new EmailAddress(from ?? _configuration["Email:DefaultFrom"], _configuration["Email:DefaultFromName"]);
                // var toAddress = new EmailAddress(to);
                // var msg = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, null, htmlBody);
                // var response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
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