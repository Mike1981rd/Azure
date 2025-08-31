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

        // Supported formats
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".avif" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi" };
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB

        public MediaUploadController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
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
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "images");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Optimize image if needed (optional - requires ImageSharp or similar)
                // await OptimizeImage(filePath);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fileUrl = $"{baseUrl}/uploads/images/{fileName}";

                return Ok(new 
                { 
                    url = fileUrl,
                    fileName = fileName,
                    size = file.Length,
                    type = "image"
                });
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
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fileUrl = $"{baseUrl}/uploads/videos/{fileName}";

                return Ok(new 
                { 
                    url = fileUrl,
                    fileName = fileName,
                    size = file.Length,
                    type = "video"
                });
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
        public IActionResult DeleteMedia(string type, string fileName)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", type);
                var filePath = Path.Combine(uploadPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { message = "File deleted successfully" });
                }

                return NotFound(new { error = "File not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete file", details = ex.Message });
            }
        }

        [HttpGet("list")]
        public IActionResult ListMedia([FromQuery] string type = "all")
        {
            try
            {
                var uploads = new List<object>();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                if (type == "images" || type == "all")
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, "uploads", "images");
                    if (Directory.Exists(imagePath))
                    {
                        var imageFiles = Directory.GetFiles(imagePath)
                            .Select(f => new FileInfo(f))
                            .OrderByDescending(f => f.CreationTime)
                            .Select(f => new
                            {
                                url = $"{baseUrl}/uploads/images/{f.Name}",
                                fileName = f.Name,
                                size = f.Length,
                                type = "image",
                                createdAt = f.CreationTime
                            });
                        uploads.AddRange(imageFiles);
                    }
                }

                if (type == "videos" || type == "all")
                {
                    var videoPath = Path.Combine(_environment.WebRootPath, "uploads", "videos");
                    if (Directory.Exists(videoPath))
                    {
                        var videoFiles = Directory.GetFiles(videoPath)
                            .Select(f => new FileInfo(f))
                            .OrderByDescending(f => f.CreationTime)
                            .Select(f => new
                            {
                                url = $"{baseUrl}/uploads/videos/{f.Name}",
                                fileName = f.Name,
                                size = f.Length,
                                type = "video",
                                createdAt = f.CreationTime
                            });
                        uploads.AddRange(videoFiles);
                    }
                }

                return Ok(new { files = uploads });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to list files", details = ex.Message });
            }
        }
    }
}