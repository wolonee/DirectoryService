using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record DeleteFileCommand(Guid FileId) : ICommand;
