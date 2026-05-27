using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _context;

    public GetLocationsHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetLocationsResponse, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var locationsQuery = _context.LocationsRead;

        var result = await locationsQuery
            .Select(l => new GetLocationDto
            {
                Id = l.Id,
                Name = l.Name.Value,
                City = l.Address.City,
                Country = l.Address.Country,
                Street = l.Address.Street,
                Timezone = l.Timezone.Value,
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetLocationsResponse(result);
    }
}