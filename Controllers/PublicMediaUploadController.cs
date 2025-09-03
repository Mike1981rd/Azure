using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/public/media")]
    [AllowAnonymous]
    public class PublicMediaUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".avif" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi" };
        private readonly long _maxImageSize = 10 * 1024 * 1024; // 10MB
        private readonly long _maxVideoSize = 100 * 1024 * 1024; // 100MB

        public PublicMediaUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("media")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Public upload (image/video)", Description = "Uploads an image or video file without auth for website widget")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (_allowedImageExtensions.Contains(ext))
            {
                if (file.Length > _maxImageSize)
                    return BadRequest(new { error = $"Image exceeds {_maxImageSize / 1024 / 1024}MB limit" });
                return await SaveFileAsync(file, "images", "image");
            }

            if (_allowedVideoExtensions.Contains(ext))
            {
                if (file.Length > _maxVideoSize)
                    return BadRequest(new { error = $"Video exceeds {_maxVideoSize / 1024 / 1024}MB limit" });
                return await SaveFileAsync(file, "videos", "video");
            }

            return BadRequest(new { error = "Unsupported file type" });
        }

        private async Task<IActionResult> SaveFileAsync(IFormFile file, string folder, string type)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadPath);
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var name = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploadPath, name);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var url = $"{baseUrl}/uploads/{folder}/{name}";
                return Ok(new { url, fileName = name, size = file.Length, type });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
            }
        }
    }
}

