using Microsoft.AspNetCore.Http;

namespace WebsiteBuilderAPI.Services.Storage
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder);
        Task<bool> DeleteAsync(string fileIdentifierOrUrl, string folder);
        Task<IReadOnlyList<string>> ListAsync(string folder, int max = 100);
        string GetPublicUrl(string relativePathOrIdentifier, string folder);
    }
}

