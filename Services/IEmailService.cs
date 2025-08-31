using System.Threading.Tasks;

namespace WebsiteBuilderAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendEmailAsync(string to, string subject, string htmlBody, string from);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendReservationConfirmationAsync(string email, int reservationId);
        Task SendAccountCreatedEmailAsync(string email, string username, string temporaryPassword);
    }
}