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
        private readonly WebsiteBuilderAPI.Services.Storage.IStorageService _storage;

        public UploadService(
            IWebHostEnvironment environment,
            ILogger<UploadService> logger,
            IHttpContextAccessor httpContextAccessor,
            WebsiteBuilderAPI.Services.Storage.IStorageService storage)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _storage = storage;
        }

        public async Task<string> UploadAvatarAsync(IFormFile file)
        {
            try
            {
                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
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
                    
                    await using var ms = new MemoryStream();
                    await image.SaveAsJpegAsync(ms, encoder);
                    ms.Position = 0;
                    var url = await _storage.UploadAsync(ms, uniqueFileName, "image/jpeg", "avatars");
                    _logger.LogInformation($"Avatar uploaded and processed successfully: {url}");
                    return url;
                }
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
                // OPCIÓN 1: Guardar SIN PROCESAR para PNG (mantiene transparencia garantizada)
                if (originalExtension == ".png")
                {
                    await using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;
                    var url = await _storage.UploadAsync(ms, uniqueFileName, "image/png", "logos");
                    _logger.LogInformation($"PNG uploaded without processing to preserve transparency");
                    return url;
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
                        
                    await using var ms2 = new MemoryStream();
                    // Choose encoder based on extension
                    var lower = originalExtension.ToLowerInvariant();
                    if (lower == ".jpg" || lower == ".jpeg")
                    {
                        var enc = new JpegEncoder { Quality = 85 };
                        await image.SaveAsJpegAsync(ms2, enc);
                    }
                    else if (lower == ".webp")
                    {
                        var enc = new WebpEncoder { Quality = 85 };
                        await image.SaveAsWebpAsync(ms2, enc);
                    }
                    else if (lower == ".gif")
                    {
                        var enc = new GifEncoder();
                        await image.SaveAsGifAsync(ms2, enc);
                    }
                    else
                    {
                        var enc = new JpegEncoder { Quality = 85 };
                        await image.SaveAsJpegAsync(ms2, enc);
                    }
                    ms2.Position = 0;
                        var url = await _storage.UploadAsync(ms2, uniqueFileName, file.ContentType, "logos");
                        _logger.LogInformation($"Imagen subida exitosamente: {url}");
                        return url;
                    }
                }
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
                
                await _storage.DeleteAsync(fileUrl, "logos");
                _logger.LogInformation($"Imagen eliminada: {fileUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen");
                throw;
            }
        }
    }
}
