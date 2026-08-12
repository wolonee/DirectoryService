using FileService.Domain;
using FileService.Domain.S3Entities;

namespace FileService.Core.Caching;

public static class MediaAssetCacheKeys
{
    private const string PREFIX = "file-service:download-url";

    public static string DownloadUrl(StorageKey finalKey) => $"{PREFIX}:{finalKey.Value}"; 
}