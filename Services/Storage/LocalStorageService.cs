using Microsoft.AspNetCore.Http;

namespace WebsiteBuilderAPI.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;
        private readonly string? _rootOverride;

        public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor http, IConfiguration configuration)
        {
            _env = env;
            _http = http;
            // Allow overriding the storage root (e.g., Render persistent disk mounted at /data)
            // Priority: config Storage:Local:Root -> env UPLOADS_ROOT_PATH -> env PERSISTENT_UPLOADS_PATH
            _rootOverride = configuration["Storage:Local:Root"]
                ?? Environment.GetEnvironmentVariable("UPLOADS_ROOT_PATH")
                ?? Environment.GetEnvironmentVariable("PERSISTENT_UPLOADS_PATH");
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            var baseRoot = !string.IsNullOrWhiteSpace(_rootOverride)
                ? _rootOverride!
                : (_env.WebRootPath ?? _env.ContentRootPath);
            var uploadPath = Path.Combine(baseRoot, "uploads", folder ?? string.Empty);
            Directory.CreateDirectory(uploadPath);

            var unique = string.IsNullOrEmpty(Path.GetExtension(fileName))
                ? $"{Guid.NewGuid()}"
                : $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(uploadPath, unique);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs);
            }

            var req = _http.HttpContext?.Request;
            var baseUrl = $"{req?.Scheme}://{req?.Host}";
            // Public URL always served under /uploads/* which is mapped via StaticFiles in Program.cs
            return $"{baseUrl}/uploads/{folder}/{unique}";
        }

        public Task<bool> DeleteAsync(string fileIdentifierOrUrl, string folder)
        {
            try
            {
                string fileName = fileIdentifierOrUrl;
                if (fileIdentifierOrUrl.StartsWith("http"))
                {
                    var uri = new Uri(fileIdentifierOrUrl);
                    fileName = Path.GetFileName(uri.LocalPath);
                }
                var baseRoot = !string.IsNullOrWhiteSpace(_rootOverride)
                    ? _rootOverride!
                    : (_env.WebRootPath ?? _env.ContentRootPath);
                var full = Path.Combine(baseRoot, "uploads", folder ?? string.Empty, fileName);
                if (File.Exists(full)) File.Delete(full);
                return Task.FromResult(true);
            }
            catch { return Task.FromResult(false); }
        }

        public Task<IReadOnlyList<string>> ListAsync(string folder, int max = 100)
        {
            var baseRoot = !string.IsNullOrWhiteSpace(_rootOverride)
                ? _rootOverride!
                : (_env.WebRootPath ?? _env.ContentRootPath);
            var path = Path.Combine(baseRoot, "uploads", folder ?? string.Empty);
            var req = _http.HttpContext?.Request;
            var baseUrl = $"{req?.Scheme}://{req?.Host}";
            if (!Directory.Exists(path))
                return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
            var list = Directory.GetFiles(path)
                .OrderByDescending(f => new FileInfo(f).CreationTimeUtc)
                .Take(max)
                .Select(f => $"{baseUrl}/uploads/{folder}/{Path.GetFileName(f)}")
                .ToList();
            return Task.FromResult((IReadOnlyList<string>)list);
        }

        public string GetPublicUrl(string relativePathOrIdentifier, string folder)
        {
            if (string.IsNullOrEmpty(relativePathOrIdentifier)) return string.Empty;
            if (relativePathOrIdentifier.StartsWith("http")) return relativePathOrIdentifier;
            var req = _http.HttpContext?.Request;
            var baseUrl = $"{req?.Scheme}://{req?.Host}";
            var rel = relativePathOrIdentifier.StartsWith("/") ? relativePathOrIdentifier : $"/uploads/{folder}/{relativePathOrIdentifier}";
            return $"{baseUrl}{rel}";
        }
    }
}
