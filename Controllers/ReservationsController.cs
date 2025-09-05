using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Services;
using Microsoft.Extensions.Logging;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public partial class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET: api/reservations
        [HttpGet]
        public async Task<IActionResult> GetReservations(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Structured logging for observability
                try
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ReservationsController>>();
                    logger.LogInformation("GetReservations called with status={Status}, startDate={Start}, endDate={End}", status, startDate, endDate);
                }
                catch { /* logging best-effort */ }

                // Obtener companyId del token (minúscula según Guardado.md)
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Fallback para single-tenant
                }

                var reservations = await _reservationService.GetReservationsByCompanyAsync(companyId, status, startDate, endDate);
                return Ok(reservations); // Devolver el array directamente
            }
            catch (Exception ex)
            {
                try
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ReservationsController>>();
                    logger.LogError(ex, "Error getting reservations. status={Status}, startDate={Start}, endDate={End}", status, startDate, endDate);
                }
                catch { }

                // Harden: devolver lista vacía para no romper el dashboard
                return Ok(Array.Empty<ReservationListDto>());
            }
        }

        // GET: api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var reservation = await _reservationService.GetReservationByIdAsync(id, companyId);
                
                if (reservation == null)
                {
                    return NotFound(new { success = false, message = $"Reservación #{id} no encontrada" });
                }

                return Ok(new { success = true, data = reservation, message = "Reservación obtenida exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al obtener la reservación", details = ex.Message });
            }
        }

        // POST: api/reservations
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var userIdClaim = User.FindFirst("id")?.Value;
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var result = await _reservationService.CreateReservationAsync(companyId, dto, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return CreatedAtAction(
                    nameof(GetReservation), 
                    new { id = result.Id }, 
                    new { success = true, data = result.Data, message = result.Message }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al crear la reservación", details = ex.Message });
            }
        }

        // PUT: api/reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _reservationService.UpdateReservationAsync(id, companyId, dto);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al actualizar la reservación", details = ex.Message });
            }
        }

        // DELETE: api/reservations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _reservationService.DeleteReservationAsync(id, companyId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al eliminar la reservación", details = ex.Message });
            }
        }

        // POST: api/reservations/{id}/confirm
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _reservationService.ConfirmReservationAsync(id, companyId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al confirmar la reservación", details = ex.Message });
            }
        }

        // POST: api/reservations/{id}/checkin
        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var userIdClaim = User.FindFirst("id")?.Value;
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var result = await _reservationService.CheckInAsync(id, companyId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al realizar check-in", details = ex.Message });
            }
        }

        // POST: api/reservations/{id}/checkout
        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var userIdClaim = User.FindFirst("id")?.Value;
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var result = await _reservationService.CheckOutAsync(id, companyId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al realizar check-out", details = ex.Message });
            }
        }

        // POST: api/reservations/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id, [FromBody] CancelReservationDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto?.Reason))
                {
                    return BadRequest(new { success = false, message = "Debe proporcionar una razón para la cancelación" });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _reservationService.CancelReservationAsync(id, companyId, dto.Reason);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al cancelar la reservación", details = ex.Message });
            }
        }

        // POST: api/reservations/{id}/payments
        [HttpPost("{id}/payments")]
        public async Task<IActionResult> AddPayment(int id, [FromBody] CreatePaymentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Datos de pago inválidos", errors = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var userIdClaim = User.FindFirst("id")?.Value;
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var result = await _reservationService.AddPaymentAsync(id, companyId, dto, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al registrar el pago", details = ex.Message });
            }
        }

        // GET: api/reservations/{id}/payments
        [HttpGet("{id}/payments")]
        public async Task<IActionResult> GetPayments(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var payments = await _reservationService.GetPaymentsByReservationAsync(id, companyId);
                
                return Ok(new { success = true, data = payments, message = $"Se encontraron {payments.Count} pagos" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al obtener los pagos", details = ex.Message });
            }
        }

        // GET: api/reservations/availability
        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability(
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] int? excludeReservationId = null)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var availableRooms = await _reservationService.GetAvailableRoomsAsync(companyId, checkIn, checkOut, excludeReservationId);
                
                return Ok(new { 
                    success = true, 
                    data = availableRooms,
                    message = $"{availableRooms.Count()} habitaciones disponibles"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al verificar disponibilidad", details = ex.Message });
            }
        }

        // GET: api/reservations/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var stats = await _reservationService.GetReservationStatsAsync(companyId, startDate, endDate);
                
                return Ok(new { success = true, data = stats, message = "Estadísticas obtenidas exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error al obtener estadísticas", details = ex.Message });
            }
        }
    }

    // DTO adicional para cancelación
    public class CancelReservationDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
