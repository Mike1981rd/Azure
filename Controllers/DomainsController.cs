using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DomainsController : ControllerBase
    {
        private readonly IDomainService _domainService;
        private readonly ILogger<DomainsController> _logger;

        public DomainsController(IDomainService domainService, ILogger<DomainsController> logger)
        {
            _domainService = domainService;
            _logger = logger;
        }

        /// <summary>
        /// Debug endpoint para verificar claims del token
        /// </summary>
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var companyId = GetCompanyIdFromToken();
            
            return Ok(new
            {
                claims = claims,
                detectedCompanyId = companyId,
                identity = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    name = User.Identity?.Name,
                    authenticationType = User.Identity?.AuthenticationType
                }
            });
        }

        /// <summary>
        /// Crear un nuevo dominio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDomain([FromBody] CreateDomainDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DomainOperationResult
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = GetModelStateErrors(),
                        ErrorCode = "VALIDATION_ERROR"
                    });
                }

                // Obtener CompanyId del token (usando minúsculas según Guardado.md)
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.CreateDomainAsync(companyId, dto, userId);

                if (result.Success)
                {
                    _logger.LogInformation($"Dominio {dto.DomainName} creado exitosamente para empresa {companyId}");
                    return CreatedAtAction(nameof(GetDomain), new { id = result.Data?.Id }, result);
                }

                _logger.LogWarning($"Error creando dominio: {result.Message}");
                return result.ErrorCode switch
                {
                    "DOMAIN_ALREADY_EXISTS" => Conflict(result),
                    "INVALID_DOMAIN_FORMAT" => BadRequest(result),
                    "DNS_VALIDATION_FAILED" => UnprocessableEntity(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado creando dominio");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Obtener todos los dominios de la empresa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDomains()
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var domains = await _domainService.GetDomainsByCompanyIdAsync(companyId);
                
                return Ok(new
                {
                    success = true,
                    data = domains,
                    count = domains.Count,
                    message = $"Se encontraron {domains.Count} dominios"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dominios");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener un dominio específico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDomain(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var domain = await _domainService.GetDomainByIdAsync(id, companyId);
                
                if (domain == null)
                {
                    return NotFound(new DomainOperationResult
                    {
                        Success = false,
                        Message = "Dominio no encontrado",
                        ErrorCode = "DOMAIN_NOT_FOUND"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = domain
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo dominio {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar un dominio
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDomain(int id, [FromBody] UpdateDomainDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DomainOperationResult
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = GetModelStateErrors(),
                        ErrorCode = "VALIDATION_ERROR"
                    });
                }

                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.UpdateDomainAsync(id, companyId, dto, userId);

                if (result.Success)
                {
                    _logger.LogInformation($"Dominio {id} actualizado exitosamente");
                    return Ok(result);
                }

                _logger.LogWarning($"Error actualizando dominio {id}: {result.Message}");
                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "DOMAIN_ALREADY_EXISTS" => Conflict(result),
                    "INVALID_DOMAIN_FORMAT" => BadRequest(result),
                    "DNS_VALIDATION_FAILED" => UnprocessableEntity(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado actualizando dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Eliminar un dominio (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDomain(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.DeleteDomainAsync(id, companyId, userId);

                if (result.Success)
                {
                    _logger.LogInformation($"Dominio {id} eliminado exitosamente");
                    return Ok(result);
                }

                _logger.LogWarning($"Error eliminando dominio {id}: {result.Message}");
                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "CANNOT_DELETE_PRIMARY" => Conflict(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado eliminando dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Establecer un dominio como primario
        /// </summary>
        [HttpPost("{id}/set-primary")]
        public async Task<IActionResult> SetPrimaryDomain(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.SetPrimaryDomainAsync(id, companyId, userId);

                if (result.Success)
                {
                    _logger.LogInformation($"Dominio {id} establecido como primario");
                    return Ok(result);
                }

                _logger.LogWarning($"Error estableciendo dominio {id} como primario: {result.Message}");
                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "DOMAIN_NOT_ACTIVE" => BadRequest(result),
                    "DOMAIN_NOT_VERIFIED" => BadRequest(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error estableciendo dominio {id} como primario");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Verificar un dominio
        /// </summary>
        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyDomain(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.VerifyDomainAsync(id, companyId, userId);

                if (result.Success)
                {
                    _logger.LogInformation($"Dominio {id} verificado exitosamente");
                    return Ok(result);
                }

                _logger.LogWarning($"Error verificando dominio {id}: {result.Message}");
                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "VERIFICATION_FAILED" => UnprocessableEntity(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Regenerar token de verificación
        /// </summary>
        [HttpPost("{id}/regenerate-token")]
        public async Task<IActionResult> RegenerateVerificationToken(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.RegenerateDomainVerificationTokenAsync(id, companyId, userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "DOMAIN_ALREADY_VERIFIED" => BadRequest(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error regenerando token para dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Obtener registros DNS de un dominio
        /// </summary>
        [HttpGet("{id}/dns-records")]
        public async Task<IActionResult> GetDnsRecords(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var records = await _domainService.GetDnsRecordsByDomainIdAsync(id, companyId);
                
                return Ok(new
                {
                    success = true,
                    data = records,
                    count = records.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo registros DNS del dominio {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Agregar registro DNS a un dominio
        /// </summary>
        [HttpPost("{id}/dns-records")]
        public async Task<IActionResult> AddDnsRecord(int id, [FromBody] CreateDnsRecordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DomainOperationResult
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = GetModelStateErrors(),
                        ErrorCode = "VALIDATION_ERROR"
                    });
                }

                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.AddDnsRecordAsync(id, companyId, dto, userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    "DNS_VALIDATION_FAILED" => BadRequest(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error agregando registro DNS al dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Actualizar registro DNS
        /// </summary>
        [HttpPut("dns-records/{recordId}")]
        public async Task<IActionResult> UpdateDnsRecord(int recordId, [FromBody] UpdateDnsRecordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DomainOperationResult
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = GetModelStateErrors(),
                        ErrorCode = "VALIDATION_ERROR"
                    });
                }

                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.UpdateDnsRecordAsync(recordId, companyId, dto, userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    "DNS_RECORD_NOT_FOUND" => NotFound(result),
                    "DNS_VALIDATION_FAILED" => BadRequest(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando registro DNS {recordId}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Eliminar registro DNS
        /// </summary>
        [HttpDelete("dns-records/{recordId}")]
        public async Task<IActionResult> DeleteDnsRecord(int recordId)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.DeleteDnsRecordAsync(recordId, companyId, userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    "DNS_RECORD_NOT_FOUND" => NotFound(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error eliminando registro DNS {recordId}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Verificar disponibilidad de nombre de dominio
        /// </summary>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckDomainAvailability([FromQuery] string domainName, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    return BadRequest(new 
                    { 
                        available = false, 
                        message = "El nombre del dominio es requerido" 
                    });
                }

                var companyId = GetCompanyIdFromToken();
                var isAvailable = await _domainService.IsDomainNameAvailableAsync(domainName, companyId, excludeId);
                
                return Ok(new
                {
                    available = isAvailable,
                    domainName = domainName,
                    message = isAvailable ? "El dominio está disponible" : "El dominio ya está registrado"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando disponibilidad del dominio {domainName}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Validar configuración de dominio
        /// </summary>
        [HttpGet("{id}/validate")]
        public async Task<IActionResult> ValidateDomainConfiguration(int id)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var errors = await _domainService.ValidateDomainConfigurationAsync(id, companyId);
                
                return Ok(new
                {
                    valid = errors.Count == 0,
                    errors = errors,
                    message = errors.Count == 0 ? "La configuración del dominio es válida" : "Se encontraron problemas en la configuración"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validando configuración del dominio {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener sugerencias de configuración DNS
        /// </summary>
        [HttpGet("dns-suggestions")]
        public async Task<IActionResult> GetDnsSuggestions([FromQuery] string domainName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    return BadRequest(new { error = "El nombre del dominio es requerido" });
                }

                var suggestions = await _domainService.GetDnsConfigurationSuggestionsAsync(domainName);
                
                return Ok(new
                {
                    domainName = domainName,
                    suggestions = suggestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo sugerencias DNS para {domainName}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener dominios próximos a expirar
        /// </summary>
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringDomains([FromQuery] int days = 30)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var domains = await _domainService.GetExpiringDomainsAsync(companyId, days);
                
                return Ok(new
                {
                    success = true,
                    data = domains,
                    count = domains.Count,
                    daysThreshold = days,
                    message = $"Se encontraron {domains.Count} dominios próximos a expirar en los próximos {days} días"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dominios próximos a expirar");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener certificados SSL próximos a expirar
        /// </summary>
        [HttpGet("expiring-ssl")]
        public async Task<IActionResult> GetExpiringSslCertificates([FromQuery] int days = 30)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var domains = await _domainService.GetExpiringSslCertificatesAsync(companyId, days);
                
                return Ok(new
                {
                    success = true,
                    data = domains,
                    count = domains.Count,
                    daysThreshold = days,
                    message = $"Se encontraron {domains.Count} certificados SSL próximos a expirar en los próximos {days} días"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo certificados SSL próximos a expirar");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar estado de un dominio
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateDomainStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var userId = User.Identity?.Name ?? "System";

                var result = await _domainService.UpdateDomainStatusAsync(id, companyId, dto.Status, userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    "DOMAIN_NOT_FOUND" => NotFound(result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando estado del dominio {id}");
                return StatusCode(500, new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        // Helper methods
        private int GetCompanyIdFromToken()
        {
            // Try multiple claim name variations for compatibility
            // CRITICAL: Check lowercase first per Guardado.md, then fallback to other formats
            var companyIdClaim = User.FindFirst("companyId")?.Value ?? 
                               User.FindFirst("CompanyId")?.Value ?? 
                               User.FindFirst("company_id")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                // Log available claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}:{c.Value}").ToArray();
                _logger.LogWarning("CompanyId no encontrado en token. Claims disponibles: {Claims}. Usando valor por defecto 1", 
                    string.Join(", ", claims));
                return 1;
            }
            
            return companyId;
        }

        private Dictionary<string, List<string>> GetModelStateErrors()
        {
            var errors = new Dictionary<string, List<string>>();
            
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    errors[key] = new List<string>();
                    foreach (var error in state.Errors)
                    {
                        errors[key].Add(error.ErrorMessage);
                    }
                }
            }
            
            return errors;
        }

        // DTO para actualizar estado
        public class UpdateStatusDto
        {
            public DomainStatus Status { get; set; }
        }
    }
}