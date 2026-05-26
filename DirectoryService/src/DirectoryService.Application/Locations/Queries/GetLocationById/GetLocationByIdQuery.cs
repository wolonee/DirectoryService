using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Query;

public record GetLocationByIdQuery(Guid Id) : IQuery;