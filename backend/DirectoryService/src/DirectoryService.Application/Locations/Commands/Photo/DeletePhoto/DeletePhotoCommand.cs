using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Commands.Photo.DeletePhoto;

public record DeletePhotoCommand(Guid LocationId) : ICommand;