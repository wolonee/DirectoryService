using DirectoryService.Application.Abstractions;

namespace FileService.Contracts;

public sealed record CompleteUploadCommand(Guid FileId) : ICommand;
