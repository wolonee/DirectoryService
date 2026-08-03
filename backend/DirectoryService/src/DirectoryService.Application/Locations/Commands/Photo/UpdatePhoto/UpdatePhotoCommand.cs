using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Commands.Photo.UpdatePhoto;

public record UpdatePhotoCommand(Guid LocationId, UpdatePhotoRequest Request) : ICommand;