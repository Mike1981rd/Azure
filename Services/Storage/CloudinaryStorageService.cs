using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace WebsiteBuilderAPI.Services.Storage
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _baseFolder;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloud = configuration["Cloudinary:CloudName"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
            var apiKey = configuration["Cloudinary:ApiKey"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            var apiSecret = configuration["Cloudinary:ApiSecret"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
            _baseFolder = configuration["Cloudinary:BaseFolder"] ?? Environment.GetEnvironmentVariable("CLOUDINARY_BASE_FOLDER") ?? "websitebuilder";

            if (string.IsNullOrWhiteSpace(cloud) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
                throw new InvalidOperationException("Cloudinary credentials are not configured.");

            var account = new Account(cloud, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = ComposeFolder(folder),
                PublicId = Path.GetFileNameWithoutExtension(fileName)
            };

            // Use ImageUploadParams when mime is image/* to get transformations in the future
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                var imageParams = new ImageUploadParams
                {
                    File = uploadParams.File,
                    Folder = uploadParams.Folder,
                    PublicId = uploadParams.PublicId,
                    UseFilename = true,
                    UniqueFilename = true,
                };
                var result = await _cloudinary.UploadAsync(imageParams);
                if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
                    return result.SecureUrl?.ToString() ?? string.Empty;
                throw new InvalidOperationException($"Cloudinary image upload failed: {result.Error?.Message}");
            }
            else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                var videoParams = new VideoUploadParams
                {
                    File = uploadParams.File,
                    Folder = uploadParams.Folder,
                    PublicId = uploadParams.PublicId,
                    UseFilename = true,
                    UniqueFilename = true,
                    ResourceType = ResourceType.Video
                };
                var result = await _cloudinary.UploadAsync(videoParams);
                if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
                    return result.SecureUrl?.ToString() ?? string.Empty;
                throw new InvalidOperationException($"Cloudinary video upload failed: {result.Error?.Message}");
            }
            else
            {
                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
                    return result.SecureUrl?.ToString() ?? string.Empty;
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message}");
            }
        }

        public async Task<bool> DeleteAsync(string fileIdentifierOrUrl, string folder)
        {
            try
            {
                string publicId = ExtractPublicId(fileIdentifierOrUrl);
                if (string.IsNullOrWhiteSpace(publicId)) return false;

                var delParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Auto
                };
                var result = await _cloudinary.DestroyAsync(delParams);
                return result.Result == "ok" || result.Result == "not found";
            }
            catch
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<string>> ListAsync(string folder, int max = 100)
        {
            try
            {
                var list = new List<string>();
                var res = await _cloudinary.ListResourcesAsync(new ListResourcesParams
                {
                    Type = "upload",
                    MaxResults = max,
                    Prefix = ComposeFolder(folder) + "/"
                });
                foreach (var r in res.Resources)
                {
                    if (!string.IsNullOrEmpty(r.SecureUrl))
                        list.Add(r.SecureUrl.ToString());
                }
                return list;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public string GetPublicUrl(string relativePathOrIdentifier, string folder)
        {
            if (string.IsNullOrWhiteSpace(relativePathOrIdentifier)) return string.Empty;
            if (relativePathOrIdentifier.StartsWith("http")) return relativePathOrIdentifier;
            // Cloudinary URLs include the full path; assume identifier passed is a publicId
            return _cloudinary.Api.UrlImgUp.Secure(true).BuildUrl(relativePathOrIdentifier);
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

