using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record StartMultipartUploadCommand(StartMultipartUploadRequest Request) : ICommand;
