using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record CancelUploadCommand(Guid FileId) : ICommand;
