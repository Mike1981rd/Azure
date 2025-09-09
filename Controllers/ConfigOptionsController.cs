using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsiteBuilderAPI.DTOs.ConfigOptions;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfigOptionsController : ControllerBase
    {
        private readonly IConfigOptionService _configOptionService;
        private readonly ILogger<ConfigOptionsController> _logger;

        public ConfigOptionsController(
            IConfigOptionService configOptionService,
            ILogger<ConfigOptionsController> logger)
        {
            _configOptionService = configOptionService;
            _logger = logger;
        }

        private int GetCompanyId()
        {
            // Intentar con minúscula primero (estándar)
            var companyIdClaim = User.FindFirst("companyId")?.Value ?? 
                                User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim))
            {
                // Si no se encuentra, usar valor por defecto
                return 1;
            }
            return int.Parse(companyIdClaim);
        }

        /// <summary>
        /// Obtiene todas las opciones de configuración por tipo
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<List<ConfigOptionDto>>> GetByType(string type)
        {
            try
            {
                var companyId = GetCompanyId();
                var options = await _configOptionService.GetByTypeAsync(companyId, type);
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo opciones por tipo {Type}", type);
                return StatusCode(500, new { error = "Error al obtener las opciones" });
            }
        }

        /// <summary>
        /// Obtiene todas las opciones de configuración
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ConfigOptionDto>>> GetAll()
        {
            try
            {
                var companyId = GetCompanyId();
                var options = await _configOptionService.GetAllAsync(companyId);
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las opciones");
                return StatusCode(500, new { error = "Error al obtener las opciones" });
            }
        }

        /// <summary>
        /// Obtiene una opción de configuración por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConfigOptionDto>> GetById(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var option = await _configOptionService.GetByIdAsync(companyId, id);
                
                if (option == null)
                    return NotFound(new { error = "Opción no encontrada" });
                
                return Ok(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo opción {Id}", id);
                return StatusCode(500, new { error = "Error al obtener la opción" });
            }
        }

        /// <summary>
        /// Crea una nueva opción de configuración
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ConfigOptionDto>> Create(CreateConfigOptionDto dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var option = await _configOptionService.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando opción");
                return StatusCode(500, new { error = "Error al crear la opción" });
            }
        }

        /// <summary>
        /// Actualiza una opción de configuración
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ConfigOptionDto>> Update(int id, UpdateConfigOptionDto dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var option = await _configOptionService.UpdateAsync(companyId, id, dto);
                
                if (option == null)
                    return NotFound(new { error = "Opción no encontrada" });
                
                return Ok(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando opción {Id}", id);
                return StatusCode(500, new { error = "Error al actualizar la opción" });
            }
        }

        /// <summary>
        /// Elimina una opción de configuración
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var deleted = await _configOptionService.DeleteAsync(companyId, id);
                
                if (!deleted)
                    return NotFound(new { error = "Opción no encontrada" });
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando opción {Id}", id);
                return StatusCode(500, new { error = "Error al eliminar la opción" });
            }
        }

        /// <summary>
        /// Incrementa el contador de uso de una opción
        /// </summary>
        [HttpPost("increment-usage")]
        public async Task<IActionResult> IncrementUsage([FromBody] IncrementUsageDto dto)
        {
            try
            {
                var companyId = GetCompanyId();
                await _configOptionService.IncrementUsageAsync(companyId, dto.Type, dto.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementando uso para {Type}/{Value}", dto.Type, dto.Value);
                return StatusCode(500, new { error = "Error al actualizar el contador" });
            }
        }

        /// <summary>
        /// Inicializa las opciones por defecto para la empresa
        /// </summary>
        [HttpPost("initialize-defaults")]
        public async Task<IActionResult> InitializeDefaults()
        {
            try
            {
                var companyId = GetCompanyId();
                await _configOptionService.InitializeDefaultOptionsAsync(companyId);
                return Ok(new { message = "Opciones por defecto inicializadas correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inicializando opciones por defecto");
                return StatusCode(500, new { error = "Error al inicializar las opciones" });
            }
        }

        /// <summary>
        /// Public endpoint: Get config options by type for a specific company (no auth required)
        /// </summary>
        [HttpGet("public/company/{companyId}/type/{type}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ConfigOptionDto>>> GetPublicByType(int companyId, string type)
        {
            try
            {
                _logger.LogInformation("Public request for ConfigOptions: companyId={CompanyId}, type={Type}", companyId, type);
                var options = await _configOptionService.GetByTypeAsync(companyId, type);
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public options for company {CompanyId} type {Type}", companyId, type);
                return StatusCode(500, new { error = "Error retrieving options" });
            }
        }

        /// <summary>
        /// Public endpoint: Get all config options for a specific company (no auth required)
        /// </summary>
        [HttpGet("public/company/{companyId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ConfigOptionDto>>> GetPublicAll(int companyId)
        {
            try
            {
                _logger.LogInformation("Public request for all ConfigOptions: companyId={CompanyId}", companyId);
                var options = await _configOptionService.GetAllAsync(companyId);
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all public options for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "Error retrieving options" });
            }
        }
    }

    public class IncrementUsageDto
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}