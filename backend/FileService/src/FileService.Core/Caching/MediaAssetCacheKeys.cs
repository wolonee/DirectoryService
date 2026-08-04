using FileService.Domain;

namespace FileService.Core.Caching;

public static class MediaAssetCacheKeys
{
    private const string PREFIX = "file-service:download-url";

    public static string DownloadUrl(StorageKey finalKey) => $"{PREFIX}:{finalKey.Value}"; 
}