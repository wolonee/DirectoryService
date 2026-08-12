using System.Text.Json;
using FileService.Contracts;
using FileService.Contracts.Features.Simple.GetMediaAsset;
using FileService.Contracts.Features.Simple.GetMediaAssets;
using FileService.Contracts.Shared;
using Xunit;

namespace FileService.Communications.UnitTests;

public sealed class PublicContractSurfaceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void GetMediaAssetResponse_JsonShape_RemainsStable()
    {
        var response = new GetMediaAssetResponse(
            Guid.Empty,
            Guid.Empty,
            "course",
            "ready",
            "preview",
            "image/png",
            "course-cover",
            1024,
            new ObjectMetadataDto(1024, "image/png", "etag", null, DateTime.UnixEpoch),
            null);

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonOptions));
        string[] properties = document.RootElement.EnumerateObject().Select(property => property.Name).ToArray();

        Assert.Equal(
            ["fileId", "entityId", "ownerContext", "status", "assetType", "contentType", "usageType", "size", "storage", "contentUrl"],
            properties);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("contentUrl").ValueKind);
    }

    [Fact]
    public void GetMediaAssetsRequest_JsonShape_RemainsStable()
    {
        var request = new GetMediaAssetsRequest([Guid.Empty]);

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(request, JsonOptions));
        string[] properties = document.RootElement.EnumerateObject().Select(property => property.Name).ToArray();

        Assert.Equal(["fileIds"], properties);
        Assert.Equal(1, document.RootElement.GetProperty("fileIds").GetArrayLength());
    }

    [Fact]
    public void GetMediaAssetsResponse_JsonShape_RemainsStable()
    {
        var response = new GetMediaAssetsResponse([
            new GetMediaAssetDto(Guid.Empty, "ready", "image/png", null),
        ]);

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonOptions));
        JsonElement item = document.RootElement.GetProperty("mediaAssets")[0];

        Assert.Equal(
            ["id", "status", "contentType", "contentUrl"],
            item.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("contentUrl").ValueKind);
    }

    [Fact]
    public void ContractsAssembly_DoesNotReferenceFileServiceInternals()
    {
        string[] references = typeof(GetMediaAssetResponse)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("FileService.Core", references);
        Assert.DoesNotContain("FileService.Domain", references);
        Assert.DoesNotContain("FileService.Infrastructure.S3", references);
        Assert.DoesNotContain("FileService.Infrastructure.Postgres", references);
        Assert.DoesNotContain("FileService.Web", references);
        Assert.Null(typeof(GetMediaAssetResponse).Assembly.GetType("FileService.Contracts.DeleteObjectResponseDto"));
    }
}
