namespace FileService.Contracts;

public record InitiateUploadResponse
{
    public Guid FileId { get; init; }
    public PresignedUploadDto Upload { get; init; } = null!;
}
