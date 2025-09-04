using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly WebsiteBuilderAPI.Services.Storage.IStorageService _storage;

        // Supported formats
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".avif" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi" };
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB

        public MediaUploadController(IWebHostEnvironment environment, IConfiguration configuration, WebsiteBuilderAPI.Services.Storage.IStorageService storage)
        {
            _environment = environment;
            _configuration = configuration;
            _storage = storage;
        }

        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Upload an image file", Description = "Uploads an image file to the server")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            if (file.Length > _maxFileSize)
                return BadRequest(new { error = $"File size exceeds {_maxFileSize / 1024 / 1024}MB limit" });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
                return BadRequest(new { error = $"Invalid file type. Allowed types: {string.Join(", ", _allowedImageExtensions)}" });

            try
            {
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var url = await _storage.UploadAsync(ms, $"{Guid.NewGuid()}{extension}", file.ContentType, "images");
                return Ok(new { url, fileName = Path.GetFileName(url), size = file.Length, type = "image" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
            }
        }

        [HttpPost("video")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Upload a video file", Description = "Uploads a video file to the server")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            if (file.Length > _maxFileSize * 2) // 100MB for videos
                return BadRequest(new { error = $"File size exceeds {_maxFileSize * 2 / 1024 / 1024}MB limit" });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedVideoExtensions.Contains(extension))
                return BadRequest(new { error = $"Invalid file type. Allowed types: {string.Join(", ", _allowedVideoExtensions)}" });

            try
            {
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var url = await _storage.UploadAsync(ms, $"{Guid.NewGuid()}{extension}", file.ContentType, "videos");
                return Ok(new { url, fileName = Path.GetFileName(url), size = file.Length, type = "video" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
            }
        }

        [HttpPost("media")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Upload a media file", Description = "Uploads an image or video file to the server")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Determine if it's image or video
            if (_allowedImageExtensions.Contains(extension))
            {
                return await UploadImage(file);
            }
            else if (_allowedVideoExtensions.Contains(extension))
            {
                return await UploadVideo(file);
            }
            else
            {
                var allAllowed = _allowedImageExtensions.Concat(_allowedVideoExtensions);
                return BadRequest(new { error = $"Invalid file type. Allowed types: {string.Join(", ", allAllowed)}" });
            }
        }

        [HttpDelete("{type}/{fileName}")]
        public async Task<IActionResult> DeleteMedia(string type, string fileName)
        {
            try
            {
                var ok = await _storage.DeleteAsync(fileName, type);
                if (ok) return Ok(new { message = "File deleted successfully" });
                return NotFound(new { error = "File not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete file", details = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListMedia([FromQuery] string type = "all")
        {
            try
            {
                var files = new List<object>();
                if (type == "images" || type == "all")
                {
                    var urls = await _storage.ListAsync("images", 100);
                    files.AddRange(urls.Select(u => new { url = u, fileName = Path.GetFileName(u), type = "image" }));
                }
                if (type == "videos" || type == "all")
                {
                    var urls = await _storage.ListAsync("videos", 100);
                    files.AddRange(urls.Select(u => new { url = u, fileName = Path.GetFileName(u), type = "video" }));
                }
                return Ok(new { files });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to list files", details = ex.Message });
            }
        }
    }
}
