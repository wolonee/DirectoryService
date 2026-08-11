namespace FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;

public record PartETagDto(int PartNumber, string ETag);