namespace FileService.Contracts;

public sealed record MultipartPartUploadDto(
    int PartNumber,
    string UploadUrl);
