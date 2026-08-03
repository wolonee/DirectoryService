namespace DirectoryService.Contracts.Locations.Responses;

public sealed record LocationPhotoResponse(
    Guid AssetId,
    string Availability,
    string ContentType,
    string? ContentUrl,
    DateTime AttachedAt);