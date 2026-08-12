namespace FileService.Contracts.Features.Simple.InitiateUpload;

public record InitiateUploadResponse
{
    public Guid FileId { get; init; }
    public PresignedUploadDto Upload { get; init; } = null!;
}
