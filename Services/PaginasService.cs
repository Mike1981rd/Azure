using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Common;
using WebsiteBuilderAPI.DTOs.Paginas;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class PaginasService : IPaginasService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaginasService> _logger;

        public PaginasService(ApplicationDbContext context, ILogger<PaginasService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DTOs.Common.ApiResponse<PagedResult<PaginaResponseDto>>> GetPaginasAsync(
            int companyId, 
            int page = 1, 
            int pageSize = 10,
            string? search = null,
            string? publishStatus = null,
            bool? isVisible = null)
        {
            try
            {
                var query = _context.Paginas
                    .Include(p => p.CreatedBy)
                    .Include(p => p.UpdatedBy)
                    .Where(p => p.CompanyId == companyId);

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p => 
                        p.Title.ToLower().Contains(search) || 
                        p.Slug.ToLower().Contains(search) ||
                        (p.MetaDescription != null && p.MetaDescription.ToLower().Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(publishStatus))
                {
                    query = query.Where(p => p.PublishStatus == publishStatus);
                }

                if (isVisible.HasValue)
                {
                    query = query.Where(p => p.IsVisible == isVisible.Value);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginas = await query
                    .OrderByDescending(p => p.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => MapToResponseDto(p))
                    .ToListAsync();

                var result = new PagedResult<PaginaResponseDto>
                {
                    Items = paginas,
                    TotalCount = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return DTOs.Common.ApiResponse<PagedResult<PaginaResponseDto>>.SuccessResponse(
                    result,
                    $"Se encontraron {totalItems} páginas"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener páginas para la empresa {CompanyId}", companyId);
                return DTOs.Common.ApiResponse<PagedResult<PaginaResponseDto>>.ErrorResponse(
                    "Error al obtener las páginas. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<PaginaResponseDto>> GetPaginaByIdAsync(int companyId, int id)
        {
            try
            {
                var pagina = await _context.Paginas
                    .Include(p => p.CreatedBy)
                    .Include(p => p.UpdatedBy)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                        $"No se encontró la página con ID {id}"
                    );
                }

                return DTOs.Common.ApiResponse<PaginaResponseDto>.SuccessResponse(
                    MapToResponseDto(pagina),
                    "Página obtenida exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener página {Id} para empresa {CompanyId}", id, companyId);
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Error al obtener la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<PaginaResponseDto>> GetPaginaBySlugAsync(int companyId, string slug)
        {
            try
            {
                var pagina = await _context.Paginas
                    .Include(p => p.CreatedBy)
                    .Include(p => p.UpdatedBy)
                    .FirstOrDefaultAsync(p => p.Slug == slug && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                        $"No se encontró ninguna página con la URL '{slug}'"
                    );
                }

                return DTOs.Common.ApiResponse<PaginaResponseDto>.SuccessResponse(
                    MapToResponseDto(pagina),
                    "Página obtenida exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener página por slug {Slug} para empresa {CompanyId}", slug, companyId);
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Error al obtener la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<PaginaResponseDto>> CreatePaginaAsync(int companyId, int userId, CreatePaginaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar si el slug ya existe
                var slugExists = await _context.Paginas
                    .AnyAsync(p => p.Slug == dto.Slug && p.CompanyId == companyId);

                if (slugExists)
                {
                    return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                        $"Ya existe una página con la URL '{dto.Slug}'. Por favor, elija otra URL."
                    );
                }

                var pagina = new Pagina
                {
                    CompanyId = companyId,
                    Title = dto.Title,
                    Slug = dto.Slug.ToLower(),
                    Content = dto.Content,
                    IsVisible = dto.IsVisible,
                    PublishStatus = dto.PublishStatus,
                    PublishedAt = dto.PublishedAt?.ToUniversalTime(),
                    ScheduledPublishAt = dto.ScheduledPublishAt?.ToUniversalTime(),
                    Template = dto.Template,
                    MetaTitle = dto.MetaTitle,
                    MetaDescription = dto.MetaDescription,
                    MetaKeywords = dto.MetaKeywords,
                    OgImage = dto.OgImage,
                    OgTitle = dto.OgTitle,
                    OgDescription = dto.OgDescription,
                    AllowSearchEngines = dto.AllowSearchEngines,
                    CanonicalUrl = dto.CanonicalUrl,
                    Robots = dto.Robots,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId
                };

                // Si el estado es published y no tiene fecha, establecerla
                if (pagina.PublishStatus == "published" && !pagina.PublishedAt.HasValue)
                {
                    pagina.PublishedAt = DateTime.UtcNow;
                }

                _context.Paginas.Add(pagina);
                await _context.SaveChangesAsync();

                // Recargar con relaciones
                await _context.Entry(pagina)
                    .Reference(p => p.CreatedBy)
                    .LoadAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Página '{Title}' creada exitosamente con ID {Id} para empresa {CompanyId}",
                    pagina.Title, pagina.Id, companyId
                );

                return DTOs.Common.ApiResponse<PaginaResponseDto>.SuccessResponse(
                    MapToResponseDto(pagina),
                    $"La página '{pagina.Title}' se ha creado exitosamente"
                );
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error de base de datos al crear página");
                
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Error al guardar la página en la base de datos. Verifique que todos los campos sean válidos."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear página para empresa {CompanyId}", companyId);
                
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Ocurrió un error inesperado al crear la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<PaginaResponseDto>> UpdatePaginaAsync(int companyId, int userId, int id, UpdatePaginaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var pagina = await _context.Paginas
                    .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                        $"No se encontró la página con ID {id}"
                    );
                }

                // Verificar slug si se está cambiando
                if (!string.IsNullOrWhiteSpace(dto.Slug) && dto.Slug != pagina.Slug)
                {
                    var slugExists = await _context.Paginas
                        .AnyAsync(p => p.Slug == dto.Slug && p.CompanyId == companyId && p.Id != id);

                    if (slugExists)
                    {
                        return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                            $"Ya existe otra página con la URL '{dto.Slug}'. Por favor, elija otra URL."
                        );
                    }
                }

                // Guardar título anterior para el mensaje
                var oldTitle = pagina.Title;

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    pagina.Title = dto.Title;

                if (!string.IsNullOrWhiteSpace(dto.Slug))
                    pagina.Slug = dto.Slug.ToLower();

                if (dto.Content != null)
                    pagina.Content = dto.Content == "" ? null : dto.Content;

                if (dto.IsVisible.HasValue)
                    pagina.IsVisible = dto.IsVisible.Value;

                if (!string.IsNullOrWhiteSpace(dto.PublishStatus))
                {
                    var oldStatus = pagina.PublishStatus;
                    pagina.PublishStatus = dto.PublishStatus;
                    
                    // Si cambia a published, establecer fecha si no existe
                    if (dto.PublishStatus == "published" && !pagina.PublishedAt.HasValue)
                    {
                        pagina.PublishedAt = DateTime.UtcNow;
                    }
                }

                if (dto.PublishedAt.HasValue)
                    pagina.PublishedAt = dto.PublishedAt.Value.ToUniversalTime();

                if (dto.ScheduledPublishAt.HasValue)
                    pagina.ScheduledPublishAt = dto.ScheduledPublishAt.Value.ToUniversalTime();

                if (!string.IsNullOrWhiteSpace(dto.Template))
                    pagina.Template = dto.Template;

                // Actualizar campos SEO
                if (dto.MetaTitle != null)
                    pagina.MetaTitle = dto.MetaTitle == "" ? null : dto.MetaTitle;

                if (dto.MetaDescription != null)
                    pagina.MetaDescription = dto.MetaDescription == "" ? null : dto.MetaDescription;

                if (dto.MetaKeywords != null)
                    pagina.MetaKeywords = dto.MetaKeywords == "" ? null : dto.MetaKeywords;

                if (dto.OgImage != null)
                    pagina.OgImage = dto.OgImage == "" ? null : dto.OgImage;

                if (dto.OgTitle != null)
                    pagina.OgTitle = dto.OgTitle == "" ? null : dto.OgTitle;

                if (dto.OgDescription != null)
                    pagina.OgDescription = dto.OgDescription == "" ? null : dto.OgDescription;

                if (dto.AllowSearchEngines.HasValue)
                    pagina.AllowSearchEngines = dto.AllowSearchEngines.Value;

                if (dto.CanonicalUrl != null)
                    pagina.CanonicalUrl = dto.CanonicalUrl == "" ? null : dto.CanonicalUrl;

                if (dto.Robots != null)
                    pagina.Robots = dto.Robots == "" ? null : dto.Robots;

                pagina.UpdatedAt = DateTime.UtcNow;
                pagina.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                // Recargar con relaciones
                await _context.Entry(pagina)
                    .Reference(p => p.CreatedBy)
                    .LoadAsync();
                await _context.Entry(pagina)
                    .Reference(p => p.UpdatedBy)
                    .LoadAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Página {Id} actualizada exitosamente para empresa {CompanyId}",
                    id, companyId
                );

                return DTOs.Common.ApiResponse<PaginaResponseDto>.SuccessResponse(
                    MapToResponseDto(pagina),
                    $"La página '{pagina.Title}' se ha actualizado correctamente"
                );
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error de base de datos al actualizar página {Id}", id);
                
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Error al actualizar la página en la base de datos. Verifique que todos los campos sean válidos."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar página {Id} para empresa {CompanyId}", id, companyId);
                
                return DTOs.Common.ApiResponse<PaginaResponseDto>.ErrorResponse(
                    "Ocurrió un error inesperado al actualizar la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse> DeletePaginaAsync(int companyId, int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var pagina = await _context.Paginas
                    .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse.ErrorResponse(
                        $"No se encontró la página con ID {id}"
                    );
                }

                var title = pagina.Title;
                _context.Paginas.Remove(pagina);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Página {Id} '{Title}' eliminada para empresa {CompanyId}",
                    id, title, companyId
                );

                return DTOs.Common.ApiResponse.SuccessResponse(
                    $"La página '{title}' se ha eliminado permanentemente"
                );
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error de base de datos al eliminar página {Id}", id);
                
                return DTOs.Common.ApiResponse.ErrorResponse(
                    "No se pudo eliminar la página debido a dependencias en el sistema. Verifique que no esté siendo utilizada."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al eliminar página {Id} para empresa {CompanyId}", id, companyId);
                
                return DTOs.Common.ApiResponse.ErrorResponse(
                    "Ocurrió un error al eliminar la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse> PublishPaginaAsync(int companyId, int userId, int id)
        {
            try
            {
                var pagina = await _context.Paginas
                    .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse.ErrorResponse(
                        $"No se encontró la página con ID {id}"
                    );
                }

                if (pagina.PublishStatus == "published")
                {
                    return DTOs.Common.ApiResponse.ErrorResponse(
                        $"La página '{pagina.Title}' ya está publicada"
                    );
                }

                pagina.PublishStatus = "published";
                pagina.PublishedAt = DateTime.UtcNow;
                pagina.UpdatedAt = DateTime.UtcNow;
                pagina.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Página {Id} publicada exitosamente para empresa {CompanyId}",
                    id, companyId
                );

                return DTOs.Common.ApiResponse.SuccessResponse(
                    $"La página '{pagina.Title}' se ha publicado exitosamente y ya está disponible en el sitio"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar página {Id}", id);
                return DTOs.Common.ApiResponse.ErrorResponse(
                    "Error al publicar la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse> UnpublishPaginaAsync(int companyId, int userId, int id)
        {
            try
            {
                var pagina = await _context.Paginas
                    .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

                if (pagina == null)
                {
                    return DTOs.Common.ApiResponse.ErrorResponse(
                        $"No se encontró la página con ID {id}"
                    );
                }

                if (pagina.PublishStatus != "published")
                {
                    return DTOs.Common.ApiResponse.ErrorResponse(
                        $"La página '{pagina.Title}' no está publicada actualmente"
                    );
                }

                pagina.PublishStatus = "draft";
                pagina.UpdatedAt = DateTime.UtcNow;
                pagina.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Página {Id} despublicada exitosamente para empresa {CompanyId}",
                    id, companyId
                );

                return DTOs.Common.ApiResponse.SuccessResponse(
                    $"La página '{pagina.Title}' se ha despublicado y ya no está visible en el sitio"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al despublicar página {Id}", id);
                return DTOs.Common.ApiResponse.ErrorResponse(
                    "Error al despublicar la página. Por favor, intente nuevamente."
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<bool>> CheckSlugExistsAsync(int companyId, string slug, int? excludeId = null)
        {
            try
            {
                var query = _context.Paginas
                    .Where(p => p.Slug == slug && p.CompanyId == companyId);

                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.Id != excludeId.Value);
                }

                var exists = await query.AnyAsync();

                return DTOs.Common.ApiResponse<bool>.SuccessResponse(
                    exists,
                    exists ? $"La URL '{slug}' ya está en uso" : $"La URL '{slug}' está disponible"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar slug {Slug}", slug);
                return DTOs.Common.ApiResponse<bool>.ErrorResponse(
                    "Error al verificar la disponibilidad de la URL"
                );
            }
        }

        public async Task<DTOs.Common.ApiResponse<string>> GenerateSlugAsync(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return DTOs.Common.ApiResponse<string>.ErrorResponse(
                        "El título no puede estar vacío"
                    );
                }

                var slug = GenerateSlug(title);
                
                return DTOs.Common.ApiResponse<string>.SuccessResponse(
                    slug,
                    "URL generada exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar slug para '{Title}'", title);
                return DTOs.Common.ApiResponse<string>.ErrorResponse(
                    "Error al generar la URL"
                );
            }
        }

        private static string GenerateSlug(string text)
        {
            // Convertir a minúsculas
            text = text.ToLowerInvariant();
            
            // Reemplazar caracteres especiales del español
            text = text.Replace("á", "a")
                      .Replace("é", "e")
                      .Replace("í", "i")
                      .Replace("ó", "o")
                      .Replace("ú", "u")
                      .Replace("ñ", "n")
                      .Replace("ü", "u");
            
            // Remover caracteres no alfanuméricos excepto espacios y guiones
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            
            // Reemplazar espacios múltiples con un solo espacio
            text = Regex.Replace(text, @"\s+", " ").Trim();
            
            // Reemplazar espacios con guiones
            text = text.Replace(" ", "-");
            
            // Remover guiones múltiples
            text = Regex.Replace(text, @"-+", "-");
            
            return text;
        }

        private static PaginaResponseDto MapToResponseDto(Pagina pagina)
        {
            return new PaginaResponseDto
            {
                Id = pagina.Id,
                CompanyId = pagina.CompanyId,
                Title = pagina.Title,
                Slug = pagina.Slug,
                Content = pagina.Content,
                IsVisible = pagina.IsVisible,
                PublishStatus = pagina.PublishStatus,
                PublishedAt = pagina.PublishedAt,
                ScheduledPublishAt = pagina.ScheduledPublishAt,
                Template = pagina.Template,
                MetaTitle = pagina.MetaTitle,
                MetaDescription = pagina.MetaDescription,
                MetaKeywords = pagina.MetaKeywords,
                OgImage = pagina.OgImage,
                OgTitle = pagina.OgTitle,
                OgDescription = pagina.OgDescription,
                AllowSearchEngines = pagina.AllowSearchEngines,
                CanonicalUrl = pagina.CanonicalUrl,
                Robots = pagina.Robots,
                CreatedAt = pagina.CreatedAt,
                UpdatedAt = pagina.UpdatedAt,
                CreatedByUserId = pagina.CreatedByUserId,
                UpdatedByUserId = pagina.UpdatedByUserId,
                CreatedByName = pagina.CreatedBy != null ? 
                    $"{pagina.CreatedBy.FirstName} {pagina.CreatedBy.LastName}" : null,
                UpdatedByName = pagina.UpdatedBy != null ? 
                    $"{pagina.UpdatedBy.FirstName} {pagina.UpdatedBy.LastName}" : null
            };
        }
    }
}