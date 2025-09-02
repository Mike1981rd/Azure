using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Location;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                // Get company ID from claims
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    // Si no hay companyId en el token, usar el ID 1 (como hace CompanyService)
                    companyId = 1;
                }

                var locations = await _locationService.GetLocationsByCompanyIdAsync(companyId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching locations", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var location = await _locationService.GetLocationByIdAsync(id, companyId);
                if (location == null)
                {
                    return NotFound(new { error = "Location not found" });
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching the location", details = ex.Message });
            }
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultLocation()
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var location = await _locationService.GetDefaultLocationAsync(companyId);
                if (location == null)
                {
                    return NotFound(new { error = "No default location found" });
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching the default location", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var location = await _locationService.CreateLocationAsync(companyId, dto);
                return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the location", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var location = await _locationService.UpdateLocationAsync(id, companyId, dto);
                if (location == null)
                {
                    return NotFound(new { error = "Location not found" });
                }

                return Ok(location);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the location", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var result = await _locationService.DeleteLocationAsync(id, companyId);
                if (!result)
                {
                    return NotFound(new { error = "Location not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the location", details = ex.Message });
            }
        }

        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultLocation(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Use default company ID
                }

                var result = await _locationService.SetDefaultLocationAsync(id, companyId);
                if (!result)
                {
                    return NotFound(new { error = "Location not found" });
                }

                return Ok(new { message = "Default location updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while setting the default location", details = ex.Message });
            }
        }
    }
}