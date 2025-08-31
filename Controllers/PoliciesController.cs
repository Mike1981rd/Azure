using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Policies;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Get company ID from token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var policies = await _policyService.GetAllByCompanyAsync(companyId);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener las políticas", details = ex.Message });
            }
        }

        [HttpGet("{type}")]
        [AllowAnonymous] // Allow public access for policies on website
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                // Get company ID from token or query parameter for public access
                int companyId = 1; // Default company
                
                // Try to get from query parameter first (for public access)
                if (Request.Query.ContainsKey("companyId"))
                {
                    int.TryParse(Request.Query["companyId"], out companyId);
                }
                // Otherwise, get from authenticated user if available
                else if (User.Identity?.IsAuthenticated == true)
                {
                    var companyIdClaim = User.FindFirst("companyId")?.Value;
                    if (!string.IsNullOrEmpty(companyIdClaim))
                    {
                        int.TryParse(companyIdClaim, out companyId);
                    }
                }

                var policy = await _policyService.GetByTypeAsync(companyId, type);
                if (policy == null)
                {
                    return NotFound(new { error = $"Política de tipo '{type}' no encontrada" });
                }

                return Ok(policy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener la política", details = ex.Message });
            }
        }

        [HttpGet("type/{type}")]
        [AllowAnonymous] // Alternative endpoint for better clarity
        public async Task<IActionResult> GetPolicyByType(string type)
        {
            return await GetByType(type);
        }

        [HttpPut("{type}")]
        public async Task<IActionResult> Update(string type, [FromBody] UpdatePolicyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get company ID from token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var updatedPolicy = await _policyService.UpdateAsync(companyId, type, dto);
                return Ok(new { message = "Política actualizada exitosamente", policy = updatedPolicy });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al actualizar la política", details = ex.Message });
            }
        }

        [HttpPost("initialize")]
        public async Task<IActionResult> InitializePolicies()
        {
            try
            {
                // Get company ID from token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                await _policyService.InitializePoliciesForCompanyAsync(companyId);
                return Ok(new { message = "Políticas inicializadas exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al inicializar las políticas", details = ex.Message });
            }
        }
    }
}