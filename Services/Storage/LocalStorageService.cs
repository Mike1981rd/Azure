using Microsoft.AspNetCore.Http;

namespace WebsiteBuilderAPI.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;

        public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor http)
        {
            _env = env;
            _http = http;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            var webRoot = _env.WebRootPath ?? _env.ContentRootPath;
            var uploadPath = Path.Combine(webRoot, "uploads", folder ?? string.Empty);
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
                var webRoot = _env.WebRootPath ?? _env.ContentRootPath;
                var full = Path.Combine(webRoot, "uploads", folder ?? string.Empty, fileName);
                if (File.Exists(full)) File.Delete(full);
                return Task.FromResult(true);
            }
            catch { return Task.FromResult(false); }
        }

        public Task<IReadOnlyList<string>> ListAsync(string folder, int max = 100)
        {
            var webRoot = _env.WebRootPath ?? _env.ContentRootPath;
            var path = Path.Combine(webRoot, "uploads", folder ?? string.Empty);
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

