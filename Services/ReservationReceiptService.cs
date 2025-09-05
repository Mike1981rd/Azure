using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IReservationReceiptService
    {
        Task<byte[]> GenerateReceiptPdfAsync(int reservationId, int companyId);
    }

    public class ReservationReceiptService : IReservationReceiptService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservationReceiptService> _logger;
        private readonly IWebHostEnvironment _env;

        public ReservationReceiptService(ApplicationDbContext context, ILogger<ReservationReceiptService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(int reservationId, int companyId)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.CompanyId == companyId);
            if (reservation == null) throw new InvalidOperationException("Reservation not found");

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId) ?? new Company { Name = "Company" };
            var primaryColorHex = company.PrimaryColor ?? "#22c55e";
            var primary = Colors.Green.Medium;
            try { primary = Color.FromHex(primaryColorHex); } catch { }

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontColor(Colors.Grey.Darken3));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(company.Name).FontSize(20).Bold().FontColor(primary);
                            col.Item().Text(company.Address ?? "");
                            col.Item().Text(company.City != null ? $"{company.City}, {company.Country}" : company.Country ?? "");
                        });

                        row.ConstantItem(160).Column(col =>
                        {
                            var code = $"RES{reservation.Id:D6}";
                            col.Item().AlignRight().Text("Reservation Receipt").FontSize(16).Bold();
                            col.Item().AlignRight().Text(code).FontSize(12).FontColor(primary);
                            col.Item().AlignRight().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text("Guest").Bold();
                        col.Item().Text($"{reservation.Customer?.FullName}  •  {reservation.Customer?.Email}");

                        col.Item().PaddingTop(10).Text("Reservation Details").Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });
                            t.Cell().Text("Room").SemiBold(); t.Cell().Text(reservation.Room?.Name ?? "");
                            t.Cell().Text("Check-in").SemiBold(); t.Cell().Text(reservation.CheckInDate.ToString("yyyy-MM-dd"));
                            t.Cell().Text("Check-out").SemiBold(); t.Cell().Text(reservation.CheckOutDate.ToString("yyyy-MM-dd"));
                            t.Cell().Text("Nights").SemiBold(); t.Cell().Text(reservation.NumberOfNights.ToString());
                            t.Cell().Text("Guests").SemiBold(); t.Cell().Text(reservation.NumberOfGuests.ToString());
                            t.Cell().Text("Status").SemiBold(); t.Cell().Text(reservation.Status);
                        });

                        var paid = reservation.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
                        var balance = reservation.TotalAmount - paid;

                        col.Item().PaddingTop(10).Text("Payment Summary").Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(1); });
                            t.Cell().Text("Total Amount").SemiBold(); t.Cell().Text($"$ {reservation.TotalAmount:F2}");
                            t.Cell().Text("Total Paid").SemiBold(); t.Cell().Text($"$ {paid:F2}");
                            t.Cell().Text("Balance").SemiBold(); t.Cell().Text($"$ {balance:F2}");
                        });

                        if (reservation.Payments.Any())
                        {
                            col.Item().PaddingTop(10).Text("Payments").Bold();
                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(2);
                                });
                                t.Header(h =>
                                {
                                    h.Cell().Text("Date").SemiBold();
                                    h.Cell().Text("Amount").SemiBold();
                                    h.Cell().Text("Status").SemiBold();
                                    h.Cell().Text("Transaction Id").SemiBold();
                                });
                                foreach (var p in reservation.Payments.OrderBy(p => p.PaymentDate))
                                {
                                    t.Cell().Text(p.PaymentDate.ToString("yyyy-MM-dd HH:mm"));
                                    t.Cell().Text($"$ {p.Amount:F2}");
                                    t.Cell().Text(p.Status);
                                    t.Cell().Text(p.TransactionId ?? "-");
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text($"{company.Name} • {DateTime.UtcNow:yyyy}").FontColor(Colors.Grey.Darken2);
                });
            }).GeneratePdf();

            return bytes;
        }
    }
}

