using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    public partial class ReservationsController
    {
        private readonly IReservationReceiptService _reservationReceiptService;

        /// <summary>
        /// Exporta una reservación a PDF (recibo)
        /// </summary>
        [HttpGet("{id}/export-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportReservationToPdf(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);
                var pdf = await _reservationReceiptService.GenerateReceiptPdfAsync(id, companyId);
                return File(pdf, "application/pdf", $"Reserva_{id:D6}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al exportar la reservación a PDF", details = ex.Message });
            }
        }
    }
}

