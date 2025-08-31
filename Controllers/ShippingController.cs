using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.Shipping;
using WebsiteBuilderAPI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly ILogger<ShippingController> _logger;

        public ShippingController(IShippingService shippingService, ILogger<ShippingController> logger)
        {
            _shippingService = shippingService;
            _logger = logger;
        }

        #region Shipping Zones

        /// <summary>
        /// Test endpoint to check if company exists
        /// </summary>
        [HttpGet("test-company")]
        public async Task<ActionResult> TestCompany()
        {
            try
            {
                var company = await _shippingService.GetCurrentCompanyAsync();
                if (company == null)
                {
                    return Ok(new { hasCompany = false, message = "No company found in database" });
                }
                return Ok(new { hasCompany = true, companyId = company.Id, companyName = company.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing company");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all shipping zones for the current company
        /// </summary>
        [HttpGet("zones")]
        //[RequirePermission("shipping", "read")] // Temporalmente deshabilitado
        public async Task<ActionResult<List<ShippingZoneDto>>> GetAllZones()
        {
            try
            {
                var zones = await _shippingService.GetAllZonesAsync();
                return Ok(zones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping zones");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific shipping zone by ID
        /// </summary>
        [HttpGet("zones/{id}")]
        //[RequirePermission("shipping", "read")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingZoneDto>> GetZoneById(int id)
        {
            try
            {
                var zone = await _shippingService.GetZoneByIdAsync(id);
                if (zone == null)
                {
                    return NotFound(new { message = "Shipping zone not found" });
                }
                return Ok(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping zone {ZoneId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new shipping zone
        /// </summary>
        [HttpPost("zones")]
        //[RequirePermission("shipping", "create")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingZoneDto>> CreateZone([FromBody] CreateShippingZoneDto request)
        {
            try
            {
                _logger.LogInformation("CreateZone endpoint called");
                _logger.LogInformation("Request data - Name: {Name}, ZoneType: {ZoneType}, Countries: {Countries}, IsActive: {IsActive}", 
                    request?.Name, request?.ZoneType, 
                    request?.Countries != null ? string.Join(",", request.Countries) : "null", 
                    request?.IsActive);

                if (request == null)
                {
                    _logger.LogError("Request is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Calling CreateZoneAsync service method");
                var zone = await _shippingService.CreateZoneAsync(request);
                _logger.LogInformation("Zone created successfully with ID: {ZoneId}", zone.Id);
                
                return CreatedAtAction(nameof(GetZoneById), new { id = zone.Id }, zone);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException in CreateZone");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "InvalidOperationException in CreateZone");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating shipping zone. Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing shipping zone
        /// </summary>
        [HttpPut("zones/{id}")]
        //[RequirePermission("shipping", "update")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingZoneDto>> UpdateZone(int id, [FromBody] UpdateShippingZoneDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var zone = await _shippingService.UpdateZoneAsync(id, request);
                if (zone == null)
                {
                    return NotFound(new { message = "Shipping zone not found" });
                }
                return Ok(zone);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping zone {ZoneId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a shipping zone
        /// </summary>
        [HttpDelete("zones/{id}")]
        //[RequirePermission("shipping", "delete")] // Temporalmente deshabilitado
        public async Task<ActionResult> DeleteZone(int id)
        {
            try
            {
                var result = await _shippingService.DeleteZoneAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Shipping zone not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shipping zone {ZoneId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Shipping Rates

        /// <summary>
        /// Add a rate to a shipping zone
        /// </summary>
        [HttpPost("zones/{zoneId}/rates")]
        //[RequirePermission("shipping", "create")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingRateDto>> AddRateToZone(int zoneId, [FromBody] CreateShippingRateDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rate = await _shippingService.AddRateToZoneAsync(zoneId, request);
                if (rate == null)
                {
                    return NotFound(new { message = "Shipping zone not found" });
                }
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rate to zone {ZoneId}", zoneId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update a shipping rate
        /// </summary>
        [HttpPut("zones/{zoneId}/rates/{rateId}")]
        //[RequirePermission("shipping", "update")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingRateDto>> UpdateRate(int zoneId, int rateId, [FromBody] UpdateShippingRateDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rate = await _shippingService.UpdateRateAsync(zoneId, rateId, request);
                if (rate == null)
                {
                    return NotFound(new { message = "Shipping rate not found" });
                }
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rate {RateId} in zone {ZoneId}", rateId, zoneId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a shipping rate
        /// </summary>
        [HttpDelete("zones/{zoneId}/rates/{rateId}")]
        //[RequirePermission("shipping", "delete")] // Temporalmente deshabilitado
        public async Task<ActionResult> DeleteRate(int zoneId, int rateId)
        {
            try
            {
                var result = await _shippingService.DeleteRateAsync(zoneId, rateId);
                if (!result)
                {
                    return NotFound(new { message = "Shipping rate not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rate {RateId} from zone {ZoneId}", rateId, zoneId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Bulk update shipping zones and rates
        /// </summary>
        [HttpPut("zones/bulk-update")]
        //[RequirePermission("shipping", "update")] // Temporalmente deshabilitado
        public async Task<ActionResult<List<ShippingZoneDto>>> BulkUpdateZones([FromBody] BulkUpdateShippingDto request)
        {
            try
            {
                _logger.LogInformation("BulkUpdateZones called with {ZoneCount} zones", request?.Zones?.Count ?? 0);
                
                if (request == null)
                {
                    _logger.LogError("BulkUpdateZones: request is null");
                    return BadRequest(new { message = "Request body is required" });
                }
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    _logger.LogError("BulkUpdateZones ModelState invalid: {Errors}", string.Join(", ", errors));
                    return BadRequest(ModelState);
                }

                var zones = await _shippingService.BulkUpdateZonesAsync(request);
                return Ok(zones);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Duplicate a shipping zone
        /// </summary>
        [HttpPost("zones/{id}/duplicate")]
        //[RequirePermission("shipping", "create")] // Temporalmente deshabilitado
        public async Task<ActionResult<ShippingZoneDto>> DuplicateZone(int id)
        {
            try
            {
                var zone = await _shippingService.DuplicateZoneAsync(id);
                if (zone == null)
                {
                    return NotFound(new { message = "Shipping zone not found" });
                }
                return Ok(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating zone {ZoneId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Reorder shipping zones
        /// </summary>
        [HttpPut("zones/reorder")]
        //[RequirePermission("shipping", "update")] // Temporalmente deshabilitado
        public async Task<ActionResult> ReorderZones([FromBody] List<int> zoneIds)
        {
            try
            {
                await _shippingService.ReorderZonesAsync(zoneIds);
                return Ok(new { message = "Zones reordered successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering zones");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion
    }
}