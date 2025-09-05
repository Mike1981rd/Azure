using System.Threading.Tasks;

namespace WebsiteBuilderAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendEmailAsync(string to, string subject, string htmlBody, string from);
        Task SendEmailAsync(string to, string subject, string htmlBody, string? from, IEnumerable<EmailAttachment>? attachments);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendReservationConfirmationAsync(string email, int reservationId);
        Task SendAccountCreatedEmailAsync(string email, string username, string temporaryPassword);
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}
