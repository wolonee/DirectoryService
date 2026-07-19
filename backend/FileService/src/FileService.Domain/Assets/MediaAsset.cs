namespace FileService.Domain;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }
    
    public MediaData MediaData { get; protected set; }

    public AssetType AssetType { get; protected set; }
    
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    
    public StorageKey Key { get; protected set; }
    
    public MediaOwner Owner { get; protected set; }
    
    public MediaStatus Status { get; protected set; }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType type,
        MediaOwner owner,
        StorageKey key)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        AssetType = type;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Owner = owner;
        Key = key;
    }
}
