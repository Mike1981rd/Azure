using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Paginas;
using WebsiteBuilderAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaginasController : ControllerBase
    {
        private readonly IPaginasService _paginasService;
        private readonly ILogger<PaginasController> _logger;

        public PaginasController(IPaginasService paginasService, ILogger<PaginasController> logger)
        {
            _paginasService = paginasService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las páginas con paginación y filtros
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaginas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? publishStatus = null,
            [FromQuery] bool? isVisible = null)
        {
            try
            {
                // Obtener CompanyId del token (con fallback para single-tenant)
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Fallback para single-tenant
                }

                // Validar parámetros de paginación
                if (page < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El número de página debe ser mayor a 0"
                    });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El tamaño de página debe estar entre 1 y 100"
                    });
                }

                var result = await _paginasService.GetPaginasAsync(companyId, page, pageSize, search, publishStatus, isVisible);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return StatusCode(500, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en GetPaginas");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al obtener las páginas",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtiene una página por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaginaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El ID de la página debe ser mayor a 0"
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _paginasService.GetPaginaByIdAsync(companyId, id);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en GetPaginaById para ID {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener la página con ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtiene una página por su slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous] // Permitir acceso público para visualización
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaginaBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El slug de la página no puede estar vacío"
                    });
                }

                // Para consultas públicas, usar company ID por defecto
                int companyId = 1;
                
                // Si el usuario está autenticado, usar su company
                if (User.Identity?.IsAuthenticated == true)
                {
                    var companyIdClaim = User.FindFirst("companyId")?.Value;
                    if (!string.IsNullOrEmpty(companyIdClaim))
                    {
                        int.TryParse(companyIdClaim, out companyId);
                    }
                }

                var result = await _paginasService.GetPaginaBySlugAsync(companyId, slug);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                // Si es consulta pública, verificar que la página esté visible y publicada
                if (User.Identity?.IsAuthenticated != true)
                {
                    if (result.Data != null && (!result.Data.IsVisible || result.Data.PublishStatus != "published"))
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "La página solicitada no está disponible"
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en GetPaginaBySlug para slug {Slug}", slug);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la página",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Crea una nueva página
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePagina([FromBody] CreatePaginaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos proporcionados no son válidos. Por favor, revise los errores.",
                        errors = errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                // Buscar el ID del usuario en los claims estándar
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("userId")?.Value 
                    ?? User.FindFirst("id")?.Value;
                    
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario de los claims. Claims disponibles: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                        
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo identificar el usuario actual"
                    });
                }

                var result = await _paginasService.CreatePaginaAsync(companyId, userId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(
                    nameof(GetPaginaById),
                    new { id = result.Data?.Id },
                    result
                );
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Error de validación al crear página");
                return BadRequest(new
                {
                    success = false,
                    message = vex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al crear página");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al crear la página. Por favor, intente nuevamente.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Actualiza una página existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePagina(int id, [FromBody] UpdatePaginaDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El ID de la página debe ser mayor a 0"
                    });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos proporcionados no son válidos. Por favor, revise los errores.",
                        errors = errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                // Buscar el ID del usuario en los claims estándar
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("userId")?.Value 
                    ?? User.FindFirst("id")?.Value;
                    
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario de los claims. Claims disponibles: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                        
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo identificar el usuario actual"
                    });
                }

                var result = await _paginasService.UpdatePaginaAsync(companyId, userId, id, dto);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("No se encontró"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Error de validación al actualizar página {Id}", id);
                return BadRequest(new
                {
                    success = false,
                    message = vex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al actualizar página {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al actualizar la página. Por favor, intente nuevamente.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Elimina una página
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePagina(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El ID de la página debe ser mayor a 0"
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _paginasService.DeletePaginaAsync(companyId, id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("No se encontró"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al eliminar página {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error al eliminar la página. Por favor, intente nuevamente.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Publica una página
        /// </summary>
        [HttpPost("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PublishPagina(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El ID de la página debe ser mayor a 0"
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                // Buscar el ID del usuario en los claims estándar
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("userId")?.Value 
                    ?? User.FindFirst("id")?.Value;
                    
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario de los claims. Claims disponibles: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                        
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo identificar el usuario actual"
                    });
                }

                var result = await _paginasService.PublishPaginaAsync(companyId, userId, id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("No se encontró"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar página {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al publicar la página. Por favor, intente nuevamente.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Despublica una página
        /// </summary>
        [HttpPost("{id}/unpublish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnpublishPagina(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El ID de la página debe ser mayor a 0"
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                // Buscar el ID del usuario en los claims estándar
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("userId")?.Value 
                    ?? User.FindFirst("id")?.Value;
                    
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario de los claims. Claims disponibles: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                        
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo identificar el usuario actual"
                    });
                }

                var result = await _paginasService.UnpublishPaginaAsync(companyId, userId, id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("No se encontró"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al despublicar página {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al despublicar la página. Por favor, intente nuevamente.",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica si un slug ya existe
        /// </summary>
        [HttpGet("check-slug")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckSlugExists(
            [FromQuery][Required] string slug,
            [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El slug no puede estar vacío"
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _paginasService.CheckSlugExistsAsync(companyId, slug, excludeId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar slug {Slug}", slug);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al verificar la disponibilidad del slug",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Genera un slug a partir de un título
        /// </summary>
        [HttpPost("generate-slug")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateSlug([FromBody] GenerateSlugRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Title))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El título no puede estar vacío"
                    });
                }

                var result = await _paginasService.GenerateSlugAsync(request.Title);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar slug para '{Title}'", request?.Title);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al generar el slug",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }

    public class GenerateSlugRequest
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; } = string.Empty;
    }
}