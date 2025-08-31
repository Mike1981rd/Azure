using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs.Availability;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        private int GetCompanyId()
        {
            Console.WriteLine($"[AVAILABILITY CONTROLLER DEBUG] Getting CompanyId from token...");
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine($"[AVAILABILITY CONTROLLER DEBUG] All claims in token: {string.Join(", ", allClaims)}");
            
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            Console.WriteLine($"[AVAILABILITY CONTROLLER DEBUG] companyId claim value: {companyIdClaim ?? "NULL"}");
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                Console.WriteLine($"[AVAILABILITY CONTROLLER DEBUG] Failed to parse companyId, using default value 1");
                companyId = 1;
            }
            else
            {
                Console.WriteLine($"[AVAILABILITY CONTROLLER DEBUG] Successfully parsed CompanyId: {companyId}");
            }
            return companyId;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId not found in token");
            return int.Parse(userIdClaim);
        }

        [HttpGet("grid")]
        public async Task<ActionResult<AvailabilityGridDto>> GetAvailabilityGrid(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? roomId = null)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.GetAvailabilityGridAsync(companyId, startDate, endDate, roomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la grilla de disponibilidad", error = ex.Message });
            }
        }

        [HttpPost("check")]
        public async Task<ActionResult<CheckAvailabilityResponseDto>> CheckAvailability([FromBody] CheckAvailabilityRequestDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.CheckRoomAvailabilityAsync(companyId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar disponibilidad", error = ex.Message });
            }
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult> GetRoomAvailability(
            int roomId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.GetRoomAvailabilityAsync(companyId, roomId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener disponibilidad de la habitación", error = ex.Message });
            }
        }

        [HttpGet("public/room/{roomId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicRoomAvailability(
            int roomId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? companyId = null)
        {
            try
            {
                Console.WriteLine($"[AVAILABILITY PUBLIC DEBUG] roomId: {roomId}, startDate: {startDate} (Kind: {startDate.Kind}), endDate: {endDate} (Kind: {endDate.Kind}), companyId: {companyId}");
                
                // Convert dates to UTC to avoid PostgreSQL timezone issues
                var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
                var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
                
                Console.WriteLine($"[AVAILABILITY PUBLIC DEBUG] Converted to UTC - startDate: {startDateUtc} (Kind: {startDateUtc.Kind}), endDate: {endDateUtc} (Kind: {endDateUtc.Kind})");
                
                // Use provided companyId or default to 1
                var company = companyId ?? 1;
                Console.WriteLine($"[AVAILABILITY PUBLIC DEBUG] Using companyId: {company}");
                
                var result = await _availabilityService.GetRoomAvailabilityAsync(company, roomId, startDateUtc, endDateUtc);
                
                Console.WriteLine($"[AVAILABILITY PUBLIC DEBUG] Result retrieved successfully with {result?.Count ?? 0} dates");
                
                // Log some sample data for debugging
                if (result != null && result.Any())
                {
                    var unavailableDates = result.Where(r => !r.IsAvailable || r.HasReservation || r.IsBlocked).Take(5);
                    if (unavailableDates.Any())
                    {
                        Console.WriteLine($"[AVAILABILITY PUBLIC DEBUG] Found {unavailableDates.Count()} unavailable dates (showing first 5):");
                        foreach (var date in unavailableDates)
                        {
                            Console.WriteLine($"  - {date.Date:yyyy-MM-dd}: Available={date.IsAvailable}, HasReservation={date.HasReservation}, IsBlocked={date.IsBlocked}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[AVAILABILITY PUBLIC DEBUG] All dates are available - no reservations or blocks found");
                    }
                }
                
                // Return the array directly as the frontend expects
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AVAILABILITY PUBLIC ERROR] Exception: {ex.Message}");
                Console.WriteLine($"[AVAILABILITY PUBLIC ERROR] Stack: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error al obtener disponibilidad de la habitación", error = ex.Message, details = ex.ToString() });
            }
        }

        [HttpPut("room/{roomId}/date/{date}")]
        public async Task<ActionResult> UpdateRoomAvailability(
            int roomId,
            string date,
            [FromBody] UpdateRoomAvailabilityDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var parsedDate = DateTime.Parse(date);
                var result = await _availabilityService.UpdateRoomAvailabilityAsync(
                    companyId, roomId, parsedDate, request.IsAvailable, request.CustomPrice, request.MinNights);
                
                if (result)
                    return Ok(new { message = "Disponibilidad actualizada exitosamente" });
                
                return BadRequest(new { message = "No se pudo actualizar la disponibilidad" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar disponibilidad", error = ex.Message });
            }
        }

        [HttpPost("bulk-update")]
        public async Task<ActionResult> BulkUpdateAvailability([FromBody] BulkAvailabilityUpdateDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.BulkUpdateAvailabilityAsync(companyId, request);
                
                if (result)
                    return Ok(new { message = "Disponibilidad actualizada exitosamente" });
                
                return BadRequest(new { message = "No se pudo actualizar la disponibilidad" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar disponibilidad masiva", error = ex.Message });
            }
        }

        [HttpPost("block-periods")]
        public async Task<ActionResult<BlockPeriodDto>> CreateBlockPeriod([FromBody] CreateBlockPeriodDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var result = await _availabilityService.CreateBlockPeriodAsync(companyId, userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear período de bloqueo", error = ex.Message });
            }
        }

        [HttpGet("block-periods")]
        public async Task<ActionResult> GetBlockPeriods(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.GetBlockPeriodsAsync(companyId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener períodos de bloqueo", error = ex.Message });
            }
        }

        [HttpPut("block-periods/{id}")]
        public async Task<ActionResult> UpdateBlockPeriod(int id, [FromBody] CreateBlockPeriodDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.UpdateBlockPeriodAsync(companyId, id, request);
                
                if (result)
                    return Ok(new { message = "Período de bloqueo actualizado exitosamente" });
                
                return NotFound(new { message = "Período de bloqueo no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar período de bloqueo", error = ex.Message });
            }
        }

        [HttpDelete("block-periods/{id}")]
        public async Task<ActionResult> DeleteBlockPeriod(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.DeleteBlockPeriodAsync(companyId, id);
                
                if (result)
                    return Ok(new { message = "Período de bloqueo eliminado exitosamente" });
                
                return NotFound(new { message = "Período de bloqueo no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar período de bloqueo", error = ex.Message });
            }
        }

        [HttpPost("rules")]
        public async Task<ActionResult<AvailabilityRuleDto>> CreateAvailabilityRule([FromBody] UpdateAvailabilityRuleDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.CreateAvailabilityRuleAsync(companyId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear regla de disponibilidad", error = ex.Message });
            }
        }

        [HttpGet("rules")]
        public async Task<ActionResult> GetAvailabilityRules([FromQuery] int? roomId = null)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.GetAvailabilityRulesAsync(companyId, roomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener reglas de disponibilidad", error = ex.Message });
            }
        }

        [HttpPut("rules/{id}")]
        public async Task<ActionResult> UpdateAvailabilityRule(int id, [FromBody] UpdateAvailabilityRuleDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.UpdateAvailabilityRuleAsync(companyId, id, request);
                
                if (result)
                    return Ok(new { message = "Regla de disponibilidad actualizada exitosamente" });
                
                return NotFound(new { message = "Regla de disponibilidad no encontrada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar regla de disponibilidad", error = ex.Message });
            }
        }

        [HttpDelete("rules/{id}")]
        public async Task<ActionResult> DeleteAvailabilityRule(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.DeleteAvailabilityRuleAsync(companyId, id);
                
                if (result)
                    return Ok(new { message = "Regla de disponibilidad eliminada exitosamente" });
                
                return NotFound(new { message = "Regla de disponibilidad no encontrada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar regla de disponibilidad", error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<OccupancyStatsDto>> GetOccupancyStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.GetOccupancyStatsAsync(companyId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas de ocupación", error = ex.Message });
            }
        }

        [HttpPost("sync")]
        public async Task<ActionResult> SyncAvailabilityWithReservations()
        {
            try
            {
                var companyId = GetCompanyId();
                await _availabilityService.SyncAvailabilityWithReservationsAsync(companyId);
                return Ok(new { message = "Disponibilidad sincronizada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al sincronizar disponibilidad", error = ex.Message });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateAvailabilityRules(
            [FromQuery] int roomId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _availabilityService.ValidateAvailabilityRulesAsync(companyId, roomId, checkIn, checkOut);
                return Ok(new { isValid = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al validar reglas de disponibilidad", error = ex.Message });
            }
        }

        public class UpdateRoomAvailabilityDto
        {
            public bool IsAvailable { get; set; }
            public decimal? CustomPrice { get; set; }
            public int? MinNights { get; set; }
        }
    }
}