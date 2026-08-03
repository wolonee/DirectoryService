using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Commands.Photo.AttachPhoto;

public record AttachPhotoCommand(Guid LocationId, AttachPhotoRequest Request) : ICommand;