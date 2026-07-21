using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }
    
    public MediaData MediaData { get; protected init; } = null!;

    public AssetType AssetType { get; protected init; }
    
    public MediaUsage Usage { get; protected init; }
    
    public DateTime CreatedAt { get; protected init; }
    
    public DateTime UpdatedAt { get; protected set; }
    
    public StorageKey RawKey { get; protected init; } = null!;
    
    public StorageKey FinalKey { get; protected set; } = null!;

    public StorageKey HlsRootKey { get; protected set; } = StorageKey.None;
    
    public MediaOwner Owner { get; protected init; } = null!;
    
    public MediaStatus Status { get; protected set; }

    protected MediaAsset()
    {
    }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType type,
        MediaUsage usage,
        MediaOwner owner,
        StorageKey rawKey,
        StorageKey finalKey)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        AssetType = type;
        Usage = usage;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Owner = owner;
        RawKey = rawKey;
        FinalKey = finalKey;
    }

    private static bool IsAllowedTransition(MediaStatus currentStatus, MediaStatus newStatus) =>
        (currentStatus, newStatus) switch
        {
            (MediaStatus.UPLOADING, MediaStatus.UPLOADED) => true,
            (MediaStatus.UPLOADING, MediaStatus.FAILED) => true,
            (MediaStatus.UPLOADING, MediaStatus.DELETED) => true,
            (MediaStatus.UPLOADED, MediaStatus.READY) => true,
            (MediaStatus.UPLOADED, MediaStatus.FAILED) => true,
            (MediaStatus.UPLOADED, MediaStatus.DELETED) => true,
            (MediaStatus.READY, MediaStatus.DELETED) => true,
            (MediaStatus.FAILED, MediaStatus.DELETED) => true,
            _ => false,
        };

    public UnitResult<Error> MarkUploaded(DateTime changedAt) =>
        ChangeStatus(MediaStatus.UPLOADED, changedAt);

    public UnitResult<Error> MarkReady(StorageKey finalKey, DateTime changedAt)
    {
        if (string.IsNullOrEmpty(finalKey.FullPath))
            return Error.Validation("media.final-key.required", "Final storage key is required");

        UnitResult<Error> result = ChangeStatus(MediaStatus.READY, changedAt);
        if (result.IsSuccess)
            FinalKey = finalKey;

        return result;
    }

    public UnitResult<Error> MarkFailed(DateTime changedAt) =>
        ChangeStatus(MediaStatus.FAILED, changedAt);

    public UnitResult<Error> MarkDeleted(DateTime changedAt) =>
        ChangeStatus(MediaStatus.DELETED, changedAt);

    private UnitResult<Error> ChangeStatus(MediaStatus newStatus, DateTime changedAt)
    {
        if (Status == newStatus)
            return UnitResult.Success<Error>();
        
        if (!IsAllowedTransition(Status, newStatus))
            return Error.Conflict("media.invalid.status-transition", $"Cannot change media status from {Status} to {newStatus}");

        Status = newStatus;
        UpdatedAt = changedAt;

        return UnitResult.Success<Error>();
    }
}
