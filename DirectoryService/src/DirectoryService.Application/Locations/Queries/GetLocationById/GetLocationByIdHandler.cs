using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Query;

public class GetLocationByIdHandler : IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetLocationByIdResponse, Errors>> Handle(GetLocationByIdQuery query, CancellationToken cancellationToken = default)
    {
        var location = await _readDbContext.LocationsRead.FirstOrDefaultAsync(l => l.Id == query.Id, cancellationToken);
        if (location is null)
        {
            return GeneralErrors.NotFound().ToErrors();
        }

        return new GetLocationByIdResponse
        {
            Id = location.Id,
            Country = location.Address.Country,
            City = location.Address.City,
            Street = location.Address.Street,
            Name = location.Name.Value,
            Timezone = location.Timezone.Value,
        };
    }
}