using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class DomainService : IDomainService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DomainService> _logger;
        private readonly Regex _domainRegex = new Regex(
            @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _ipv4Regex = new Regex(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
            RegexOptions.Compiled);

        public DomainService(ApplicationDbContext context, ILogger<DomainService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DomainOperationResult> CreateDomainAsync(int companyId, CreateDomainDto dto, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                // Validar formato del dominio
                if (!_domainRegex.IsMatch(dto.DomainName))
                {
                    result.Success = false;
                    result.Message = "El formato del dominio no es válido";
                    result.Errors.Add("domainName", new List<string> { 
                        "El formato del dominio no es válido. Ejemplo: miempresa.com" 
                    });
                    result.ErrorCode = "INVALID_DOMAIN_FORMAT";
                    return result;
                }

                // Verificar si el dominio ya existe para esta empresa
                var existingDomain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.DomainName.ToLower() == dto.DomainName.ToLower() 
                        && d.CompanyId == companyId && !d.IsDeleted);
                
                if (existingDomain != null)
                {
                    result.Success = false;
                    result.Message = $"Ya existe un dominio '{dto.DomainName}' registrado para esta empresa";
                    result.Errors.Add("duplicate", new List<string> { 
                        $"El dominio '{dto.DomainName}' ya está registrado" 
                    });
                    result.ErrorCode = "DOMAIN_ALREADY_EXISTS";
                    return result;
                }

                // Si se quiere establecer como primario, verificar que no haya otro
                if (dto.IsPrimary)
                {
                    var currentPrimary = await _context.Domains
                        .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.IsPrimary && !d.IsDeleted);
                    
                    if (currentPrimary != null)
                    {
                        currentPrimary.IsPrimary = false;
                        currentPrimary.UpdatedAt = DateTime.UtcNow;
                        currentPrimary.UpdatedBy = userId;
                    }
                }

                // Crear el dominio
                var domain = new Domain
                {
                    CompanyId = companyId,
                    DomainName = dto.DomainName.ToLower(),
                    SubDomain = dto.SubDomain,
                    Status = dto.Status,
                    IsActive = dto.IsActive,
                    IsPrimary = dto.IsPrimary,
                    HasSsl = dto.HasSsl,
                    SslExpiryDate = dto.SslExpiryDate?.ToUniversalTime(),
                    SslProvider = dto.SslProvider,
                    Provider = dto.Provider,
                    ExpiryDate = dto.ExpiryDate?.ToUniversalTime(),
                    Notes = dto.Notes,
                    VerificationToken = GenerateVerificationToken(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Domains.Add(domain);
                await _context.SaveChangesAsync();

                // Agregar registros DNS si se proporcionaron
                int dnsRecordsCreated = 0;
                if (dto.DnsRecords != null && dto.DnsRecords.Any())
                {
                    var validationErrors = ValidateDnsRecords(dto.DnsRecords);
                    if (validationErrors.Any())
                    {
                        result.Success = false;
                        result.Message = "Errores en la configuración DNS";
                        result.Errors = validationErrors;
                        result.ErrorCode = "DNS_VALIDATION_FAILED";
                        
                        // Rollback: eliminar el dominio creado
                        _context.Domains.Remove(domain);
                        await _context.SaveChangesAsync();
                        return result;
                    }

                    foreach (var dnsDto in dto.DnsRecords)
                    {
                        var dnsRecord = new DnsRecord
                        {
                            DomainId = domain.Id,
                            Type = dnsDto.Type,
                            Host = dnsDto.Host,
                            Value = dnsDto.Value,
                            TTL = dnsDto.TTL,
                            Priority = dnsDto.Priority,
                            Port = dnsDto.Port,
                            Weight = dnsDto.Weight,
                            IsActive = dnsDto.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };
                        _context.DnsRecords.Add(dnsRecord);
                        dnsRecordsCreated++;
                    }
                    await _context.SaveChangesAsync();
                }

                // Cargar el dominio con sus relaciones para el response
                var createdDomain = await _context.Domains
                    .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                    .FirstOrDefaultAsync(d => d.Id == domain.Id);

                result.Success = true;
                result.Message = $"Dominio '{dto.DomainName}' creado exitosamente";
                result.Data = MapToResponseDto(createdDomain!);
                result.Details = new DomainOperationDetails
                {
                    DnsRecordsCreated = dnsRecordsCreated,
                    VerificationPending = true,
                    EstimatedPropagationTime = "24-48 horas",
                    Suggestions = new List<string>
                    {
                        "Configure los registros DNS en su proveedor de dominio",
                        "Verifique el dominio usando el token proporcionado",
                        "El SSL se activará automáticamente después de la verificación"
                    }
                };

                _logger.LogInformation($"Dominio {domain.DomainName} creado exitosamente para empresa {companyId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creando dominio para empresa {companyId}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al crear el dominio",
                    ErrorCode = "INTERNAL_ERROR",
                    Errors = new Dictionary<string, List<string>>
                    {
                        ["system"] = new List<string> { "Ocurrió un error inesperado. Por favor intente nuevamente." }
                    }
                };
            }
        }

        public async Task<DomainResponseDto?> GetDomainByIdAsync(int id, int companyId)
        {
            var domain = await _context.Domains
                .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
            
            return domain != null ? MapToResponseDto(domain) : null;
        }

        public async Task<List<DomainResponseDto>> GetDomainsByCompanyIdAsync(int companyId)
        {
            var domains = await _context.Domains
                .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                .Where(d => d.CompanyId == companyId && !d.IsDeleted)
                .OrderBy(d => d.IsPrimary ? 0 : 1)
                .ThenBy(d => d.DomainName)
                .ToListAsync();
            
            return domains.Select(MapToResponseDto).ToList();
        }

        public async Task<DomainOperationResult> UpdateDomainAsync(int id, int companyId, UpdateDomainDto dto, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .Include(d => d.DnsRecords)
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                // Validar nuevo nombre de dominio si se proporciona
                if (!string.IsNullOrEmpty(dto.DomainName) && dto.DomainName != domain.DomainName)
                {
                    if (!_domainRegex.IsMatch(dto.DomainName))
                    {
                        result.Success = false;
                        result.Message = "El formato del dominio no es válido";
                        result.Errors.Add("domainName", new List<string> { 
                            "El formato del dominio no es válido. Ejemplo: miempresa.com" 
                        });
                        result.ErrorCode = "INVALID_DOMAIN_FORMAT";
                        return result;
                    }

                    var duplicateDomain = await _context.Domains
                        .AnyAsync(d => d.DomainName.ToLower() == dto.DomainName.ToLower() 
                            && d.CompanyId == companyId && d.Id != id && !d.IsDeleted);
                    
                    if (duplicateDomain)
                    {
                        result.Success = false;
                        result.Message = $"Ya existe otro dominio '{dto.DomainName}' registrado";
                        result.Errors.Add("duplicate", new List<string> { 
                            $"El dominio '{dto.DomainName}' ya está en uso" 
                        });
                        result.ErrorCode = "DOMAIN_ALREADY_EXISTS";
                        return result;
                    }
                    
                    domain.DomainName = dto.DomainName.ToLower();
                }

                // Manejar cambio de dominio primario
                if (dto.IsPrimary.HasValue && dto.IsPrimary.Value && !domain.IsPrimary)
                {
                    var currentPrimary = await _context.Domains
                        .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.IsPrimary && d.Id != id && !d.IsDeleted);
                    
                    if (currentPrimary != null)
                    {
                        currentPrimary.IsPrimary = false;
                        currentPrimary.UpdatedAt = DateTime.UtcNow;
                        currentPrimary.UpdatedBy = userId;
                    }
                }

                // Actualizar campos
                if (dto.SubDomain != null) domain.SubDomain = dto.SubDomain;
                if (dto.Status.HasValue) domain.Status = dto.Status.Value;
                if (dto.IsActive.HasValue) domain.IsActive = dto.IsActive.Value;
                if (dto.IsPrimary.HasValue) domain.IsPrimary = dto.IsPrimary.Value;
                if (dto.HasSsl.HasValue) domain.HasSsl = dto.HasSsl.Value;
                if (dto.SslExpiryDate.HasValue) domain.SslExpiryDate = dto.SslExpiryDate.Value.ToUniversalTime();
                if (dto.SslProvider != null) domain.SslProvider = dto.SslProvider;
                if (dto.IsVerified.HasValue) domain.IsVerified = dto.IsVerified.Value;
                if (dto.VerifiedAt.HasValue) domain.VerifiedAt = dto.VerifiedAt.Value.ToUniversalTime();
                if (dto.Provider != null) domain.Provider = dto.Provider;
                if (dto.ExpiryDate.HasValue) domain.ExpiryDate = dto.ExpiryDate.Value.ToUniversalTime();
                if (dto.Notes != null) domain.Notes = dto.Notes;
                
                domain.UpdatedAt = DateTime.UtcNow;
                domain.UpdatedBy = userId;

                // Actualizar registros DNS si se proporcionaron
                int dnsRecordsCreated = 0, dnsRecordsUpdated = 0, dnsRecordsDeleted = 0;
                if (dto.DnsRecords != null)
                {
                    // Convertir UpdateDnsRecordDto a CreateDnsRecordDto para validación
                    var recordsToValidate = dto.DnsRecords.Select(r => new CreateDnsRecordDto
                    {
                        Type = r.Type,
                        Host = r.Host,
                        Value = r.Value,
                        Priority = r.Priority,
                        TTL = r.TTL
                    }).ToList();
                    
                    var validationErrors = ValidateDnsRecords(recordsToValidate);
                    if (validationErrors.Any())
                    {
                        result.Success = false;
                        result.Message = "Errores en la configuración DNS";
                        result.Errors = validationErrors;
                        result.ErrorCode = "DNS_VALIDATION_FAILED";
                        return result;
                    }

                    foreach (var dnsDto in dto.DnsRecords)
                    {
                        if (dnsDto.Id.HasValue && dnsDto.Id.Value > 0)
                        {
                            // Actualizar registro existente
                            var existingRecord = domain.DnsRecords.FirstOrDefault(r => r.Id == dnsDto.Id.Value);
                            if (existingRecord != null)
                            {
                                if (dnsDto.IsDeleted)
                                {
                                    existingRecord.IsDeleted = true;
                                    existingRecord.DeletedAt = DateTime.UtcNow;
                                    dnsRecordsDeleted++;
                                }
                                else
                                {
                                    existingRecord.Type = dnsDto.Type;
                                    existingRecord.Host = dnsDto.Host;
                                    existingRecord.Value = dnsDto.Value;
                                    existingRecord.TTL = dnsDto.TTL;
                                    existingRecord.Priority = dnsDto.Priority;
                                    existingRecord.Port = dnsDto.Port;
                                    existingRecord.Weight = dnsDto.Weight;
                                    existingRecord.IsActive = dnsDto.IsActive;
                                    existingRecord.UpdatedAt = DateTime.UtcNow;
                                    existingRecord.UpdatedBy = userId;
                                    dnsRecordsUpdated++;
                                }
                            }
                        }
                        else if (!dnsDto.IsDeleted)
                        {
                            // Crear nuevo registro
                            var newRecord = new DnsRecord
                            {
                                DomainId = domain.Id,
                                Type = dnsDto.Type,
                                Host = dnsDto.Host,
                                Value = dnsDto.Value,
                                TTL = dnsDto.TTL,
                                Priority = dnsDto.Priority,
                                Port = dnsDto.Port,
                                Weight = dnsDto.Weight,
                                IsActive = dnsDto.IsActive,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userId
                            };
                            _context.DnsRecords.Add(newRecord);
                            dnsRecordsCreated++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Recargar el dominio actualizado
                var updatedDomain = await _context.Domains
                    .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                    .FirstOrDefaultAsync(d => d.Id == domain.Id);

                result.Success = true;
                result.Message = $"Dominio '{domain.DomainName}' actualizado exitosamente";
                result.Data = MapToResponseDto(updatedDomain!);
                result.Details = new DomainOperationDetails
                {
                    DnsRecordsCreated = dnsRecordsCreated,
                    DnsRecordsUpdated = dnsRecordsUpdated,
                    DnsRecordsDeleted = dnsRecordsDeleted,
                    EstimatedPropagationTime = dnsRecordsCreated > 0 || dnsRecordsUpdated > 0 ? "4-24 horas" : null,
                    Warnings = new List<string>()
                };

                if (dnsRecordsCreated > 0 || dnsRecordsUpdated > 0)
                {
                    result.Details.Warnings.Add("Los cambios en DNS pueden tardar hasta 24 horas en propagarse");
                }

                _logger.LogInformation($"Dominio {domain.DomainName} actualizado exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando dominio {id}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al actualizar el dominio",
                    ErrorCode = "INTERNAL_ERROR",
                    Errors = new Dictionary<string, List<string>>
                    {
                        ["system"] = new List<string> { "Ocurrió un error inesperado. Por favor intente nuevamente." }
                    }
                };
            }
        }

        public async Task<DomainOperationResult> DeleteDomainAsync(int id, int companyId, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                // No permitir eliminar el dominio primario si es el único activo
                if (domain.IsPrimary && domain.IsActive)
                {
                    var otherActiveDomains = await _context.Domains
                        .AnyAsync(d => d.CompanyId == companyId && d.Id != id && d.IsActive && !d.IsDeleted);
                    
                    if (!otherActiveDomains)
                    {
                        result.Success = false;
                        result.Message = "No se puede eliminar el único dominio primario activo";
                        result.Errors.Add("primary", new List<string> { 
                            "Debe tener al menos un dominio activo. Desactive este dominio o active otro antes de eliminarlo." 
                        });
                        result.ErrorCode = "CANNOT_DELETE_PRIMARY";
                        return result;
                    }
                }

                // Soft delete
                domain.IsDeleted = true;
                domain.DeletedAt = DateTime.UtcNow;
                domain.DeletedBy = userId;
                domain.IsActive = false;
                domain.IsPrimary = false;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Dominio '{domain.DomainName}' eliminado exitosamente";
                result.Details = new DomainOperationDetails
                {
                    Warnings = new List<string> 
                    { 
                        "El dominio ha sido marcado como eliminado pero se mantiene en la base de datos para auditoría",
                        "Recuerde actualizar la configuración DNS en su proveedor"
                    }
                };

                _logger.LogInformation($"Dominio {domain.DomainName} eliminado (soft delete) por usuario {userId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error eliminando dominio {id}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al eliminar el dominio",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<DomainOperationResult> SetPrimaryDomainAsync(int id, int companyId, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                if (!domain.IsActive)
                {
                    result.Success = false;
                    result.Message = "No se puede establecer como primario un dominio inactivo";
                    result.Errors.Add("status", new List<string> { 
                        "Active el dominio antes de establecerlo como primario" 
                    });
                    result.ErrorCode = "DOMAIN_NOT_ACTIVE";
                    return result;
                }

                if (!domain.IsVerified)
                {
                    result.Success = false;
                    result.Message = "No se puede establecer como primario un dominio no verificado";
                    result.Errors.Add("verification", new List<string> { 
                        "Verifique el dominio antes de establecerlo como primario" 
                    });
                    result.ErrorCode = "DOMAIN_NOT_VERIFIED";
                    return result;
                }

                // Desactivar el dominio primario actual
                var currentPrimary = await _context.Domains
                    .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.IsPrimary && d.Id != id && !d.IsDeleted);
                
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    currentPrimary.UpdatedBy = userId;
                }

                domain.IsPrimary = true;
                domain.UpdatedAt = DateTime.UtcNow;
                domain.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Dominio '{domain.DomainName}' establecido como primario exitosamente";
                result.Data = MapToResponseDto(domain);
                result.Details = new DomainOperationDetails
                {
                    Suggestions = new List<string>
                    {
                        "El dominio primario será usado como URL principal del sitio",
                        "Los otros dominios redirigirán automáticamente al dominio primario"
                    }
                };

                _logger.LogInformation($"Dominio {domain.DomainName} establecido como primario para empresa {companyId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error estableciendo dominio {id} como primario");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al establecer el dominio como primario",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<DomainOperationResult> VerifyDomainAsync(int id, int companyId, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                if (domain.IsVerified)
                {
                    result.Success = true;
                    result.Message = "El dominio ya está verificado";
                    result.Data = MapToResponseDto(domain);
                    return result;
                }

                // Aquí iría la lógica real de verificación DNS
                // Por ahora simulamos una verificación exitosa si tiene registros DNS configurados
                bool hasRequiredDnsRecords = domain.DnsRecords
                    .Any(r => r.Type == DnsRecordType.A && r.Host == "@" && !r.IsDeleted);

                if (!hasRequiredDnsRecords)
                {
                    result.Success = false;
                    result.Message = "La verificación del dominio falló";
                    result.Errors.Add("dns", new List<string> 
                    { 
                        "No se encontraron los registros DNS requeridos",
                        "Configure al menos un registro A apuntando a la IP del servidor"
                    });
                    result.ErrorCode = "VERIFICATION_FAILED";
                    return result;
                }

                domain.IsVerified = true;
                domain.VerifiedAt = DateTime.UtcNow;
                domain.Status = DomainStatus.Active;
                domain.UpdatedAt = DateTime.UtcNow;
                domain.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Dominio '{domain.DomainName}' verificado exitosamente";
                result.Data = MapToResponseDto(domain);
                result.Details = new DomainOperationDetails
                {
                    VerificationPending = false,
                    Suggestions = new List<string>
                    {
                        "El dominio está listo para ser usado",
                        "Considere activar SSL para mayor seguridad",
                        "Puede establecer este dominio como primario si lo desea"
                    }
                };

                _logger.LogInformation($"Dominio {domain.DomainName} verificado exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando dominio {id}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al verificar el dominio",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<DomainOperationResult> RegenerateDomainVerificationTokenAsync(int id, int companyId, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                if (domain.IsVerified)
                {
                    result.Success = false;
                    result.Message = "El dominio ya está verificado";
                    result.ErrorCode = "DOMAIN_ALREADY_VERIFIED";
                    return result;
                }

                domain.VerificationToken = GenerateVerificationToken();
                domain.UpdatedAt = DateTime.UtcNow;
                domain.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Token de verificación regenerado exitosamente";
                result.Data = MapToResponseDto(domain);
                result.Details = new DomainOperationDetails
                {
                    VerificationPending = true,
                    Suggestions = new List<string>
                    {
                        $"Nuevo token de verificación: {domain.VerificationToken}",
                        "Agregue un registro TXT con este token en su proveedor DNS",
                        "La verificación puede tardar hasta 24 horas"
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error regenerando token para dominio {id}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al regenerar el token",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        // DNS Record operations
        public async Task<DomainOperationResult> AddDnsRecordAsync(int domainId, int companyId, CreateDnsRecordDto dto, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.Id == domainId && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                // Validar el registro DNS
                var validationErrors = ValidateDnsRecord(dto);
                if (validationErrors.Any())
                {
                    result.Success = false;
                    result.Message = "Error en la configuración del registro DNS";
                    result.Errors = validationErrors;
                    result.ErrorCode = "DNS_VALIDATION_FAILED";
                    return result;
                }

                var dnsRecord = new DnsRecord
                {
                    DomainId = domainId,
                    Type = dto.Type,
                    Host = dto.Host,
                    Value = dto.Value,
                    TTL = dto.TTL,
                    Priority = dto.Priority,
                    Port = dto.Port,
                    Weight = dto.Weight,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.DnsRecords.Add(dnsRecord);
                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Registro DNS {dto.Type} agregado exitosamente";
                result.Details = new DomainOperationDetails
                {
                    DnsRecordsCreated = 1,
                    EstimatedPropagationTime = "4-24 horas",
                    Suggestions = new List<string>
                    {
                        "El registro DNS ha sido agregado",
                        "Los cambios pueden tardar hasta 24 horas en propagarse globalmente"
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error agregando registro DNS al dominio {domainId}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al agregar el registro DNS",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<DomainOperationResult> UpdateDnsRecordAsync(int recordId, int companyId, UpdateDnsRecordDto dto, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var dnsRecord = await _context.DnsRecords
                    .Include(r => r.Domain)
                    .FirstOrDefaultAsync(r => r.Id == recordId && r.Domain!.CompanyId == companyId && !r.IsDeleted);
                
                if (dnsRecord == null)
                {
                    result.Success = false;
                    result.Message = "Registro DNS no encontrado";
                    result.ErrorCode = "DNS_RECORD_NOT_FOUND";
                    return result;
                }

                // Validar el registro DNS
                var validationErrors = ValidateDnsRecord(dto);
                if (validationErrors.Any())
                {
                    result.Success = false;
                    result.Message = "Error en la configuración del registro DNS";
                    result.Errors = validationErrors;
                    result.ErrorCode = "DNS_VALIDATION_FAILED";
                    return result;
                }

                dnsRecord.Type = dto.Type;
                dnsRecord.Host = dto.Host;
                dnsRecord.Value = dto.Value;
                dnsRecord.TTL = dto.TTL;
                dnsRecord.Priority = dto.Priority;
                dnsRecord.Port = dto.Port;
                dnsRecord.Weight = dto.Weight;
                dnsRecord.IsActive = dto.IsActive;
                dnsRecord.UpdatedAt = DateTime.UtcNow;
                dnsRecord.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Registro DNS actualizado exitosamente";
                result.Details = new DomainOperationDetails
                {
                    DnsRecordsUpdated = 1,
                    EstimatedPropagationTime = "4-24 horas"
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando registro DNS {recordId}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al actualizar el registro DNS",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<DomainOperationResult> DeleteDnsRecordAsync(int recordId, int companyId, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var dnsRecord = await _context.DnsRecords
                    .Include(r => r.Domain)
                    .FirstOrDefaultAsync(r => r.Id == recordId && r.Domain!.CompanyId == companyId && !r.IsDeleted);
                
                if (dnsRecord == null)
                {
                    result.Success = false;
                    result.Message = "Registro DNS no encontrado";
                    result.ErrorCode = "DNS_RECORD_NOT_FOUND";
                    return result;
                }

                dnsRecord.IsDeleted = true;
                dnsRecord.DeletedAt = DateTime.UtcNow;
                dnsRecord.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Registro DNS eliminado exitosamente";
                result.Details = new DomainOperationDetails
                {
                    DnsRecordsDeleted = 1,
                    Warnings = new List<string>
                    {
                        "El registro DNS ha sido eliminado",
                        "Recuerde actualizar la configuración en su proveedor DNS"
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error eliminando registro DNS {recordId}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al eliminar el registro DNS",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<List<DnsRecordResponseDto>> GetDnsRecordsByDomainIdAsync(int domainId, int companyId)
        {
            var records = await _context.DnsRecords
                .Include(r => r.Domain)
                .Where(r => r.DomainId == domainId && r.Domain!.CompanyId == companyId && !r.IsDeleted)
                .ToListAsync();
            
            return records.Select(MapDnsRecordToResponseDto).ToList();
        }

        // Validation operations
        public async Task<bool> IsDomainNameAvailableAsync(string domainName, int companyId, int? excludeDomainId = null)
        {
            var query = _context.Domains
                .Where(d => d.DomainName.ToLower() == domainName.ToLower() && d.CompanyId == companyId && !d.IsDeleted);
            
            if (excludeDomainId.HasValue)
            {
                query = query.Where(d => d.Id != excludeDomainId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<Dictionary<string, List<string>>> ValidateDomainConfigurationAsync(int domainId, int companyId)
        {
            var errors = new Dictionary<string, List<string>>();
            
            var domain = await _context.Domains
                .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(d => d.Id == domainId && d.CompanyId == companyId && !d.IsDeleted);
            
            if (domain == null)
            {
                errors.Add("domain", new List<string> { "Dominio no encontrado" });
                return errors;
            }

            // Validar que tenga al menos un registro A o CNAME
            var hasRequiredRecords = domain.DnsRecords
                .Any(r => (r.Type == DnsRecordType.A || r.Type == DnsRecordType.CNAME) && r.IsActive);
            
            if (!hasRequiredRecords)
            {
                errors.Add("dns", new List<string> 
                { 
                    "Se requiere al menos un registro A o CNAME activo" 
                });
            }

            // Validar SSL si está habilitado
            if (domain.HasSsl && domain.SslExpiryDate.HasValue)
            {
                if (domain.SslExpiryDate.Value < DateTime.UtcNow)
                {
                    errors.Add("ssl", new List<string> 
                    { 
                        "El certificado SSL ha expirado" 
                    });
                }
                else if (domain.SslExpiryDate.Value < DateTime.UtcNow.AddDays(30))
                {
                    errors.Add("ssl", new List<string> 
                    { 
                        $"El certificado SSL expirará pronto ({domain.SslExpiryDate.Value:yyyy-MM-dd})" 
                    });
                }
            }

            // Validar expiración del dominio
            if (domain.ExpiryDate.HasValue)
            {
                if (domain.ExpiryDate.Value < DateTime.UtcNow)
                {
                    errors.Add("expiry", new List<string> 
                    { 
                        "El dominio ha expirado" 
                    });
                }
                else if (domain.ExpiryDate.Value < DateTime.UtcNow.AddDays(30))
                {
                    errors.Add("expiry", new List<string> 
                    { 
                        $"El dominio expirará pronto ({domain.ExpiryDate.Value:yyyy-MM-dd})" 
                    });
                }
            }

            return errors;
        }

        public Task<List<string>> GetDnsConfigurationSuggestionsAsync(string domainName)
        {
            var suggestions = new List<string>
            {
                $"Configure un registro A para '@' apuntando a la IP del servidor",
                $"Configure un registro A para 'www' apuntando a la IP del servidor",
                $"Alternativamente, use un registro CNAME para 'www' apuntando a '{domainName}'",
                "Configure registros MX para recibir correos en este dominio",
                "Agregue un registro TXT para verificación del dominio",
                "Configure un registro CAA para mayor seguridad SSL"
            };

            return Task.FromResult(suggestions);
        }

        // Status operations
        public async Task<DomainOperationResult> UpdateDomainStatusAsync(int id, int companyId, DomainStatus status, string userId)
        {
            try
            {
                var result = new DomainOperationResult();
                
                var domain = await _context.Domains
                    .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId && !d.IsDeleted);
                
                if (domain == null)
                {
                    result.Success = false;
                    result.Message = "Dominio no encontrado";
                    result.ErrorCode = "DOMAIN_NOT_FOUND";
                    return result;
                }

                var previousStatus = domain.Status;
                domain.Status = status;
                domain.UpdatedAt = DateTime.UtcNow;
                domain.UpdatedBy = userId;

                // Aplicar lógica adicional según el estado
                switch (status)
                {
                    case DomainStatus.Active:
                        domain.IsActive = true;
                        break;
                    case DomainStatus.Inactive:
                    case DomainStatus.Suspended:
                    case DomainStatus.Expired:
                        domain.IsActive = false;
                        domain.IsPrimary = false;
                        break;
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Estado del dominio actualizado de '{GetStatusDisplay(previousStatus)}' a '{GetStatusDisplay(status)}'";
                result.Data = MapToResponseDto(domain);

                _logger.LogInformation($"Estado del dominio {domain.DomainName} actualizado a {status}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando estado del dominio {id}");
                return new DomainOperationResult
                {
                    Success = false,
                    Message = "Error interno al actualizar el estado del dominio",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<List<DomainResponseDto>> GetExpiringDomainsAsync(int companyId, int daysThreshold = 30)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            
            var domains = await _context.Domains
                .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                .Where(d => d.CompanyId == companyId && !d.IsDeleted 
                    && d.ExpiryDate.HasValue && d.ExpiryDate.Value <= thresholdDate)
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();
            
            return domains.Select(MapToResponseDto).ToList();
        }

        public async Task<List<DomainResponseDto>> GetExpiringSslCertificatesAsync(int companyId, int daysThreshold = 30)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            
            var domains = await _context.Domains
                .Include(d => d.DnsRecords.Where(r => !r.IsDeleted))
                .Where(d => d.CompanyId == companyId && !d.IsDeleted 
                    && d.HasSsl && d.SslExpiryDate.HasValue && d.SslExpiryDate.Value <= thresholdDate)
                .OrderBy(d => d.SslExpiryDate)
                .ToListAsync();
            
            return domains.Select(MapToResponseDto).ToList();
        }

        // Helper methods
        private DomainResponseDto MapToResponseDto(Domain domain)
        {
            var dto = new DomainResponseDto
            {
                Id = domain.Id,
                CompanyId = domain.CompanyId,
                DomainName = domain.DomainName,
                SubDomain = domain.SubDomain,
                Status = domain.Status,
                StatusDisplay = GetStatusDisplay(domain.Status),
                IsActive = domain.IsActive,
                IsPrimary = domain.IsPrimary,
                HasSsl = domain.HasSsl,
                SslExpiryDate = domain.SslExpiryDate,
                SslProvider = domain.SslProvider,
                IsVerified = domain.IsVerified,
                VerificationToken = domain.VerificationToken,
                VerifiedAt = domain.VerifiedAt,
                Provider = domain.Provider,
                ExpiryDate = domain.ExpiryDate,
                Notes = domain.Notes,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,
                CreatedBy = domain.CreatedBy,
                UpdatedBy = domain.UpdatedBy,
                DnsRecords = domain.DnsRecords?.Select(MapDnsRecordToResponseDto).ToList() ?? new List<DnsRecordResponseDto>()
            };

            // Calcular días hasta expiración
            if (domain.SslExpiryDate.HasValue)
            {
                dto.SslDaysUntilExpiry = (int)(domain.SslExpiryDate.Value - DateTime.UtcNow).TotalDays;
            }
            
            if (domain.ExpiryDate.HasValue)
            {
                dto.DaysUntilExpiry = (int)(domain.ExpiryDate.Value - DateTime.UtcNow).TotalDays;
            }

            // Determinar si requiere acción
            dto.RequiresAction = !domain.IsVerified || 
                (dto.SslDaysUntilExpiry.HasValue && dto.SslDaysUntilExpiry.Value < 30) ||
                (dto.DaysUntilExpiry.HasValue && dto.DaysUntilExpiry.Value < 30);

            // Agregar acciones pendientes
            if (!domain.IsVerified)
            {
                dto.PendingActions.Add("Verificar dominio");
            }
            
            if (dto.SslDaysUntilExpiry.HasValue && dto.SslDaysUntilExpiry.Value < 30)
            {
                dto.PendingActions.Add("Renovar certificado SSL");
            }
            
            if (dto.DaysUntilExpiry.HasValue && dto.DaysUntilExpiry.Value < 30)
            {
                dto.PendingActions.Add("Renovar dominio");
            }

            return dto;
        }

        private DnsRecordResponseDto MapDnsRecordToResponseDto(DnsRecord record)
        {
            return new DnsRecordResponseDto
            {
                Id = record.Id,
                Type = record.Type,
                TypeDisplay = record.Type.ToString(),
                Host = record.Host,
                Value = record.Value,
                TTL = record.TTL,
                TTLDisplay = FormatTTL(record.TTL),
                Priority = record.Priority,
                Port = record.Port,
                Weight = record.Weight,
                IsActive = record.IsActive,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }

        private string FormatTTL(int seconds)
        {
            if (seconds < 60) return $"{seconds} segundos";
            if (seconds < 3600) return $"{seconds / 60} minutos";
            if (seconds < 86400) return $"{seconds / 3600} horas";
            return $"{seconds / 86400} días";
        }

        private string GetStatusDisplay(DomainStatus status)
        {
            return status switch
            {
                DomainStatus.Pending => "Pendiente",
                DomainStatus.Active => "Activo",
                DomainStatus.Inactive => "Inactivo",
                DomainStatus.Suspended => "Suspendido",
                DomainStatus.Expired => "Expirado",
                DomainStatus.Transferring => "Transfiriendo",
                DomainStatus.Failed => "Fallido",
                _ => status.ToString()
            };
        }

        private string GenerateVerificationToken()
        {
            return $"verify-{Guid.NewGuid():N}";
        }

        private Dictionary<string, List<string>> ValidateDnsRecords(List<CreateDnsRecordDto> records)
        {
            var errors = new Dictionary<string, List<string>>();
            
            foreach (var record in records)
            {
                var recordErrors = ValidateDnsRecord(record);
                foreach (var error in recordErrors)
                {
                    if (!errors.ContainsKey(error.Key))
                    {
                        errors[error.Key] = new List<string>();
                    }
                    errors[error.Key].AddRange(error.Value);
                }
            }
            
            return errors;
        }

        private Dictionary<string, List<string>> ValidateDnsRecord(CreateDnsRecordDto record)
        {
            var errors = new Dictionary<string, List<string>>();
            
            // Validar según el tipo de registro
            switch (record.Type)
            {
                case DnsRecordType.A:
                    if (!_ipv4Regex.IsMatch(record.Value))
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro A requiere una dirección IPv4 válida (ej: 192.168.1.1)" 
                        });
                    }
                    break;
                    
                case DnsRecordType.AAAA:
                    // Validación básica de IPv6
                    if (!record.Value.Contains(":"))
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro AAAA requiere una dirección IPv6 válida" 
                        });
                    }
                    break;
                    
                case DnsRecordType.CNAME:
                    if (!_domainRegex.IsMatch(record.Value) && record.Value != "@")
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro CNAME requiere un nombre de dominio válido" 
                        });
                    }
                    break;
                    
                case DnsRecordType.MX:
                    if (!record.Priority.HasValue)
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro MX requiere una prioridad (0-65535)" 
                        });
                    }
                    if (!_domainRegex.IsMatch(record.Value))
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro MX requiere un servidor de correo válido" 
                        });
                    }
                    break;
                    
                case DnsRecordType.SRV:
                    if (!record.Priority.HasValue || !record.Port.HasValue || !record.Weight.HasValue)
                    {
                        errors.Add("dnsRecords", new List<string> 
                        { 
                            $"El registro SRV requiere prioridad, puerto y peso" 
                        });
                    }
                    break;
            }
            
            return errors;
        }

        private Dictionary<string, List<string>> ValidateDnsRecord(UpdateDnsRecordDto record)
        {
            // Crear un CreateDnsRecordDto temporal para reutilizar la validación
            var createDto = new CreateDnsRecordDto
            {
                Type = record.Type,
                Host = record.Host,
                Value = record.Value,
                TTL = record.TTL,
                Priority = record.Priority,
                Port = record.Port,
                Weight = record.Weight,
                IsActive = record.IsActive
            };
            
            return ValidateDnsRecord(createDto);
        }
    }
}