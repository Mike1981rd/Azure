using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace WebsiteBuilderAPI.Services
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UploadService(
            IWebHostEnvironment environment,
            ILogger<UploadService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadAvatarAsync(IFormFile file)
        {
            try
            {
                // Create avatars directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename (always save as .jpg after processing)
                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Process image with ImageSharp to fix orientation
                using (var inputStream = file.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                {
                    // AutoOrient fixes orientation based on EXIF metadata
                    image.Mutate(x => x.AutoOrient());
                    
                    // Resize avatar to standard size (300x300)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(300, 300),
                        Mode = ResizeMode.Crop
                    }));
                    
                    // Save processed image as JPEG with quality 85
                    var encoder = new JpegEncoder
                    {
                        Quality = 85
                    };
                    
                    await image.SaveAsync(filePath, encoder);
                }

                // Return full URL
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = $"{request?.Scheme}://{request?.Host}";
                var fileUrl = $"{baseUrl}/uploads/avatars/{uniqueFileName}";

                _logger.LogInformation($"Avatar uploaded and processed successfully: {fileUrl}");
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Crear directorio de uploads si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "logos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Obtener la extensión original del archivo
                var originalExtension = Path.GetExtension(file.FileName).ToLower();
                if (string.IsNullOrEmpty(originalExtension))
                {
                    // Si no tiene extensión, determinar por content type
                    originalExtension = file.ContentType.ToLower() switch
                    {
                        "image/png" => ".png",
                        "image/webp" => ".webp",
                        "image/gif" => ".gif",
                        "image/jpeg" => ".jpg",
                        "image/jpg" => ".jpg",
                        _ => ".jpg"
                    };
                }

                // Generar nombre único manteniendo la extensión original
                var uniqueFileName = $"{Guid.NewGuid()}{originalExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // OPCIÓN 1: Guardar SIN PROCESAR para PNG (mantiene transparencia garantizada)
                if (originalExtension == ".png")
                {
                    // Guardar el PNG tal cual, sin procesamiento
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    _logger.LogInformation($"PNG guardado sin procesamiento para preservar transparencia");
                }
                else
                {
                    // Para otros formatos, procesar con ImageSharp
                    using (var inputStream = file.OpenReadStream())
                    using (var image = await Image.LoadAsync(inputStream))
                    {
                        // AutoOrient corrige automáticamente la orientación basándose en los metadatos EXIF
                        image.Mutate(x => x.AutoOrient());
                        
                        // Opcional: Redimensionar si la imagen es muy grande (max 1920px de ancho)
                        if (image.Width > 1920)
                        {
                            image.Mutate(x => x.Resize(1920, 0));
                        }
                        
                        // Guardar con encoder por defecto según extensión
                        await image.SaveAsync(filePath);
                    }
                }

                // Retornar la URL completa
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = $"{request?.Scheme}://{request?.Host}";
                var fileUrl = $"{baseUrl}/uploads/logos/{uniqueFileName}";

                _logger.LogInformation($"Imagen subida exitosamente: {fileUrl}");
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen");
                throw;
            }
        }

        public async Task DeleteImageAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return;

                // Manejar URLs relativas y absolutas
                string fileName;
                if (fileUrl.StartsWith("http://") || fileUrl.StartsWith("https://"))
                {
                    var uri = new Uri(fileUrl);
                    fileName = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    // Es una ruta relativa
                    fileName = Path.GetFileName(fileUrl);
                }
                
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "logos", fileName);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation($"Imagen eliminada: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen");
                throw;
            }
        }
    }
}