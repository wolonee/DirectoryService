using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;
