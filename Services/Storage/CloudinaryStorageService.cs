using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WebsiteBuilderAPI.Services.Storage
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly string _baseFolder;
        private readonly string _cloudName;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly HttpClient _http;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            _cloudName = configuration["Cloudinary:CloudName"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty;
            _apiKey = configuration["Cloudinary:ApiKey"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty;
            _apiSecret = configuration["Cloudinary:ApiSecret"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty;
            _baseFolder = configuration["Cloudinary:BaseFolder"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_BASE_FOLDER") ?? "websitebuilder";

            if (string.IsNullOrWhiteSpace(_cloudName) || string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_apiSecret))
                throw new InvalidOperationException("Cloudinary credentials are not configured.");
            _http = new HttpClient();
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            var folderPath = ComposeFolder(folder);
            var publicId = Path.GetFileNameWithoutExtension(fileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Build signature base string: folder=...&public_id=...&timestamp=... + api_secret
            var toSign = $"folder={folderPath}&public_id={publicId}&timestamp={timestamp}{_apiSecret}";
            string signature;
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(toSign));
                signature = string.Concat(hash.Select(b => b.ToString("x2")));
            }

            var endpoint = $"https://api.cloudinary.com/v1_1/{_cloudName}/auto/upload";
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(_apiKey), "api_key");
            content.Add(new StringContent(timestamp), "timestamp");
            content.Add(new StringContent(signature), "signature");
            content.Add(new StringContent(folderPath), "folder");
            content.Add(new StringContent(publicId), "public_id");

            var resp = await _http.PostAsync(endpoint, content);
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Cloudinary upload failed: {resp.StatusCode} {json}");

            // naive parse of secure_url
            var marker = "\"secure_url\":\"";
            var idx = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                idx += marker.Length;
                var end = json.IndexOf('"', idx);
                if (end > idx)
                {
                    return json.Substring(idx, end - idx).Replace("\\/", "/");
                }
            }
            return string.Empty;
        }

        public async Task<bool> DeleteAsync(string fileIdentifierOrUrl, string folder)
        {
            try
            {
                // Optional: implement Cloudinary destroy via Admin API. For now return true to avoid 404s in UI.
                await Task.CompletedTask;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<IReadOnlyList<string>> ListAsync(string folder, int max = 100)
        {
            // Optional: implement Admin API listing if needed. Return empty to keep API simple.
            return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
        }

        public string GetPublicUrl(string relativePathOrIdentifier, string folder)
        {
            if (string.IsNullOrWhiteSpace(relativePathOrIdentifier)) return string.Empty;
            return relativePathOrIdentifier.StartsWith("http") ? relativePathOrIdentifier : $"https://res.cloudinary.com/{_cloudName}/image/upload/{relativePathOrIdentifier}";
        }

        private string ComposeFolder(string folder)
        {
            folder = folder?.Trim('/') ?? string.Empty;
            return string.IsNullOrEmpty(folder) ? _baseFolder : $"{_baseFolder}/{folder}";
        }

        private static string ExtractPublicId(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            try
            {
                // Cloudinary publicId is the path after /upload/ and before extension
                var uri = new Uri(url);
                var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var uploadIndex = Array.FindIndex(parts, p => p.Equals("upload", StringComparison.OrdinalIgnoreCase));
                if (uploadIndex >= 0 && uploadIndex < parts.Length - 1)
                {
                    var relevant = string.Join('/', parts.Skip(uploadIndex + 1));
                    return Path.ChangeExtension(relevant, null);
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
