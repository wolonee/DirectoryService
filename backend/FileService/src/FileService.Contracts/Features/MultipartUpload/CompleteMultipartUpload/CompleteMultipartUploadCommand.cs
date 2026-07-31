using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record CompleteMultipartUploadCommand(CompleteMultipartUploadRequest Request) : ICommand;
