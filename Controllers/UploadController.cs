using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IUploadService uploadService, ILogger<UploadController> logger)
        {
            _uploadService = uploadService;
            _logger = logger;
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file provided" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { error = "File must be an image (JPEG, PNG, GIF or WebP)" });
                }

                // Validate size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "File size must not exceed 5MB" });
                }

                var result = await _uploadService.UploadAvatarAsync(file);
                return Ok(new { url = result, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return StatusCode(500, new { error = "Error processing avatar" });
            }
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No se proporcionó ningún archivo" });
                }

                // Validar que sea una imagen
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { error = "El archivo debe ser una imagen (JPEG, PNG, GIF o WebP)" });
                }

                // Validar tamaño (5MB máximo)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "El archivo no debe superar los 5MB" });
                }

                var result = await _uploadService.UploadImageAsync(file);
                return Ok(new { url = result, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen");
                return StatusCode(500, new { error = "Error al procesar la imagen" });
            }
        }

        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest(new { error = "URL no proporcionada" });
                }

                await _uploadService.DeleteImageAsync(url);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen");
                return StatusCode(500, new { error = "Error al eliminar la imagen" });
            }
        }
    }
}