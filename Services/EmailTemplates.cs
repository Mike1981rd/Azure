using System.Globalization;
using System.Linq;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public static class EmailTemplates
    {
        private static string GetRoomImageHtml(Room? room)
        {
            if (room?.Images == null || !room.Images.Any())
                return "";

            var firstImage = room.Images.First();
            var imageUrl = "";
            
            if (firstImage.StartsWith("/"))
            {
                // Es path relativo, agregar dominio base
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://websitebuilder-api-staging.onrender.com";
                imageUrl = $"{apiUrl}{firstImage}";
            }
            else if (!firstImage.StartsWith("http"))
            {
                // No tiene protocolo, asumir que está en uploads
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://websitebuilder-api-staging.onrender.com";
                imageUrl = $"{apiUrl}/uploads/{firstImage}";
            }
            else
            {
                // Ya es URL completa
                imageUrl = firstImage;
            }

            return $@"<img src='{imageUrl}' alt='{room.Name}' style='width: 100%; height: 200px; object-fit: cover; border-radius: 8px; margin-bottom: 15px;' />";
        }

        public static string GenerateReservationConfirmationHtml(
            Reservation reservation,
            Customer customer,
            Room room,
            Company company,
            ReservationPayment? payment = null)
        {
            var checkInDate = reservation.CheckInDate.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
            var checkOutDate = reservation.CheckOutDate.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
            var totalAmount = reservation.TotalAmount.ToString("N2", CultureInfo.InvariantCulture);
            var confirmationNumber = $"RES{reservation.Id:D6}";
            var primaryColor = company.PrimaryColor ?? "#22c55e";

            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmación de Reservación</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f7f7f7;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
        <!-- Header -->
        <div style='background-color: {primaryColor}; padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>{company.Name}</h1>
            {(string.IsNullOrEmpty(company.Logo) ? "" : $@"<img src='{company.Logo}' alt='{company.Name}' style='max-width: 200px; max-height: 80px; margin-top: 10px;' />")}
        </div>

        <!-- Success Message -->
        <div style='background-color: {primaryColor}10; padding: 40px 30px; text-align: center;'>
            <div style='display: inline-block; background-color: {primaryColor}; border-radius: 50%; padding: 15px; margin-bottom: 20px;'>
                <span style='color: #ffffff; font-size: 48px;'>✓</span>
            </div>
            <h2 style='color: #333333; margin: 0 0 10px 0; font-size: 24px;'>¡Reservación Confirmada!</h2>
            <p style='color: #666666; margin: 0 0 20px 0;'>Tu reservación ha sido procesada exitosamente</p>
            <div style='background-color: #ffffff; border-radius: 8px; padding: 15px; display: inline-block;'>
                <p style='color: #999999; margin: 0 0 5px 0; font-size: 12px;'>Número de confirmación</p>
                <p style='color: {primaryColor}; margin: 0; font-size: 24px; font-weight: bold;'>{confirmationNumber}</p>
            </div>
        </div>

        <!-- Reservation Details -->
        <div style='padding: 30px;'>
            <h3 style='color: #333333; margin: 0 0 20px 0; font-size: 20px; border-bottom: 2px solid #eeeeee; padding-bottom: 10px;'>
                Detalles de la Reservación
            </h3>

            <!-- Room Info -->
            <div style='margin-bottom: 25px;'>
                {GetRoomImageHtml(room)}
                <h4 style='color: #333333; margin: 0 0 10px 0; font-size: 18px;'>{room?.Name ?? "Habitación"}</h4>
                <p style='color: #666666; margin: 0;'>📍 {room?.City ?? company.City ?? ""}</p>
            </div>

            <!-- Dates Grid -->
            <table style='width: 100%; margin-bottom: 25px;'>
                <tr>
                    <td style='width: 50%; padding: 15px; background-color: #f9f9f9; border-radius: 8px 0 0 8px;'>
                        <p style='color: {primaryColor}; margin: 0 0 5px 0; font-weight: bold;'>📅 Check-in</p>
                        <p style='color: #333333; margin: 0;'>{checkInDate}</p>
                        <p style='color: #999999; margin: 5px 0 0 0; font-size: 12px;'>A partir de las 3:00 PM</p>
                    </td>
                    <td style='width: 50%; padding: 15px; background-color: #f9f9f9; border-radius: 0 8px 8px 0; border-left: 2px solid #ffffff;'>
                        <p style='color: {primaryColor}; margin: 0 0 5px 0; font-weight: bold;'>📅 Check-out</p>
                        <p style='color: #333333; margin: 0;'>{checkOutDate}</p>
                        <p style='color: #999999; margin: 5px 0 0 0; font-size: 12px;'>Antes de las 12:00 PM</p>
                    </td>
                </tr>
            </table>

            <!-- Guest Information -->
            <div style='background-color: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 25px;'>
                <h4 style='color: #333333; margin: 0 0 15px 0;'>👤 Información del Huésped</h4>
                <p style='color: #666666; margin: 0 0 8px 0;'><strong>Nombre:</strong> {customer.FullName}</p>
                <p style='color: #666666; margin: 0 0 8px 0;'><strong>Email:</strong> {customer.Email}</p>
                {(string.IsNullOrEmpty(customer.Phone) ? "" : $"<p style='color: #666666; margin: 0 0 8px 0;'><strong>Teléfono:</strong> {customer.Phone}</p>")}
                <p style='color: #666666; margin: 0 0 8px 0;'><strong>Número de huéspedes:</strong> {reservation.NumberOfGuests}</p>
                <p style='color: #666666; margin: 0;'><strong>Número de noches:</strong> {reservation.NumberOfNights}</p>
            </div>

            <!-- Special Requests -->
            {(string.IsNullOrEmpty(reservation.SpecialRequests) ? "" : $@"
            <div style='background-color: #fff9e6; padding: 20px; border-radius: 8px; margin-bottom: 25px; border-left: 4px solid #ffc107;'>
                <h4 style='color: #333333; margin: 0 0 10px 0;'>📝 Solicitudes Especiales</h4>
                <p style='color: #666666; margin: 0; font-style: italic;'>{reservation.SpecialRequests}</p>
            </div>
            ")}

            <!-- Payment Summary -->
            <div style='background-color: {primaryColor}10; padding: 20px; border-radius: 8px; margin-bottom: 25px;'>
                <table style='width: 100%;'>
                    <tr>
                        <td style='color: #333333; font-size: 18px; font-weight: bold;'>Total Pagado</td>
                        <td style='text-align: right; color: {primaryColor}; font-size: 24px; font-weight: bold;'>
                            ${totalAmount}
                        </td>
                    </tr>
                    {(payment != null ? $@"
                    <tr>
                        <td style='color: #666666; padding-top: 10px;'>Método de pago:</td>
                        <td style='text-align: right; color: #666666; padding-top: 10px;'>{payment.PaymentMethod}</td>
                    </tr>
                    <tr>
                        <td style='color: #666666;'>Fecha de pago:</td>
                        <td style='text-align: right; color: #666666;'>{payment.PaymentDate:dd/MM/yyyy HH:mm}</td>
                    </tr>
                    " : "")}
                </table>
            </div>

            <!-- Important Information -->
            <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin-bottom: 25px; border: 1px solid #ffeeba;'>
                <h4 style='color: #856404; margin: 0 0 15px 0;'>⚠️ Información Importante</h4>
                <ul style='color: #856404; margin: 0; padding-left: 20px;'>
                    <li style='margin-bottom: 8px;'>Por favor presenta tu número de confirmación al hacer check-in</li>
                    <li style='margin-bottom: 8px;'>Si necesitas hacer cambios, contáctanos lo antes posible</li>
                    <li style='margin-bottom: 8px;'>Revisa nuestras políticas de cancelación</li>
                    <li>Guarda este email como comprobante de tu reservación</li>
                </ul>
            </div>
        </div>

        <!-- Footer -->
        <div style='background-color: #333333; padding: 30px; text-align: center;'>
            <h4 style='color: #ffffff; margin: 0 0 15px 0;'>¿Necesitas ayuda?</h4>
            <p style='color: #cccccc; margin: 0 0 5px 0;'>
                📞 {company.PhoneNumber ?? "+503 1234-5678"} | ✉️ {company.ContactEmail ?? company.SenderEmail ?? "reservaciones@hotel.com"}
            </p>
            {(string.IsNullOrEmpty(company.Address) ? "" : $"<p style='color: #cccccc; margin: 10px 0 0 0;'>📍 {company.Address}, {company.City}, {company.Country}</p>")}
            <p style='color: #999999; margin: 20px 0 0 0; font-size: 12px;'>
                © {DateTime.Now.Year} {company.Name}. Todos los derechos reservados.
            </p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        public static string GenerateAdminPaymentNotificationHtml(
            Reservation reservation,
            Customer customer,
            Room room,
            Company company,
            ReservationPayment payment)
        {
            var totalAmount = reservation.TotalAmount.ToString("N2", CultureInfo.InvariantCulture);
            var paymentAmount = payment.Amount.ToString("N2", CultureInfo.InvariantCulture);
            var confirmationNumber = $"RES{reservation.Id:D6}";
            var primaryColor = company.PrimaryColor ?? "#22c55e";

            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Nuevo Pago Recibido</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f7f7f7;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, {primaryColor}, {primaryColor}dd); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0 0 10px 0; font-size: 24px;'>💰 Nuevo Pago Recibido</h1>
            <p style='color: #ffffff; margin: 0; opacity: 0.9;'>Sistema de Reservaciones - {company.Name}</p>
        </div>

        <!-- Payment Alert -->
        <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 20px; margin: 20px 30px; border-radius: 8px;'>
            <h2 style='color: #155724; margin: 0 0 10px 0; font-size: 20px;'>✅ Pago Confirmado</h2>
            <p style='color: #155724; margin: 0; font-size: 24px; font-weight: bold;'>${paymentAmount}</p>
            <p style='color: #155724; margin: 5px 0 0 0;'>Método: {payment.PaymentMethod}</p>
        </div>

        <!-- Reservation Details -->
        <div style='padding: 0 30px 30px 30px;'>
            <h3 style='color: #333333; margin: 20px 0 15px 0; font-size: 18px; border-bottom: 2px solid #eeeeee; padding-bottom: 10px;'>
                Detalles de la Reservación
            </h3>

            <table style='width: 100%; border-collapse: collapse;'>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold; width: 40%;'>Confirmación #</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{confirmationNumber}</td>
                </tr>
                <tr>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Cliente</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{customer.FullName}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Email</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{customer.Email}</td>
                </tr>
                <tr>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Teléfono</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{customer.Phone ?? "No proporcionado"}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Habitación</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{room.Name}</td>
                </tr>
                <tr>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Check-in</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{reservation.CheckInDate:dd/MM/yyyy}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Check-out</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{reservation.CheckOutDate:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Noches</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{reservation.NumberOfNights}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 12px; border: 1px solid #eeeeee; font-weight: bold;'>Huéspedes</td>
                    <td style='padding: 12px; border: 1px solid #eeeeee;'>{reservation.NumberOfGuests}</td>
                </tr>
            </table>

            <!-- Payment Summary -->
            <div style='background-color: #e7f3ff; padding: 20px; border-radius: 8px; margin-top: 20px; border: 1px solid #b3d9ff;'>
                <h4 style='color: #004085; margin: 0 0 15px 0;'>💳 Resumen de Pagos</h4>
                <table style='width: 100%;'>
                    <tr>
                        <td style='color: #004085;'>Total de la Reservación:</td>
                        <td style='text-align: right; color: #004085; font-weight: bold;'>${totalAmount}</td>
                    </tr>
                    <tr>
                        <td style='color: #004085; padding-top: 8px;'>Pago Recibido:</td>
                        <td style='text-align: right; color: #28a745; font-weight: bold; padding-top: 8px;'>${paymentAmount}</td>
                    </tr>
                    <tr>
                        <td style='color: #004085; padding-top: 8px; border-top: 1px solid #b3d9ff;'>Balance Pendiente:</td>
                        <td style='text-align: right; color: #004085; font-weight: bold; padding-top: 8px; border-top: 1px solid #b3d9ff;'>
                            ${(reservation.TotalAmount - payment.Amount).ToString("N2", CultureInfo.InvariantCulture)}
                        </td>
                    </tr>
                </table>
            </div>

            <!-- Transaction Details -->
            <div style='background-color: #f9f9f9; padding: 15px; border-radius: 8px; margin-top: 20px;'>
                <p style='color: #666666; margin: 0 0 8px 0;'><strong>ID de Transacción:</strong> {payment.TransactionId ?? "N/A"}</p>
                <p style='color: #666666; margin: 0 0 8px 0;'><strong>Fecha y Hora:</strong> {payment.PaymentDate:dd/MM/yyyy HH:mm:ss}</p>
                <p style='color: #666666; margin: 0;'><strong>Estado:</strong> {payment.Status}</p>
                {(string.IsNullOrEmpty(payment.Notes) ? "" : $"<p style='color: #666666; margin: 8px 0 0 0;'><strong>Notas:</strong> {payment.Notes}</p>")}
            </div>
        </div>

        <!-- Footer -->
        <div style='background-color: #f7f7f7; padding: 20px 30px; text-align: center; border-top: 1px solid #eeeeee;'>
            <p style='color: #999999; margin: 0; font-size: 12px;'>
                Este es un mensaje automático del sistema de reservaciones de {company.Name}
            </p>
            <p style='color: #999999; margin: 5px 0 0 0; font-size: 12px;'>
                Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}
            </p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}