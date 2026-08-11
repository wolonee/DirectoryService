namespace FileService.Contracts.Features.MultipartUpload.StartMultipartUpload;

public sealed record MultipartPartUploadDto(
    int PartNumber,
    string UploadUrl);
