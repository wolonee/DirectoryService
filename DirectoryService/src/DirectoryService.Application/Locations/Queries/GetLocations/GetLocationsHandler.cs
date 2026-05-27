using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _context;
    private readonly IValidator<GetLocationsQuery> _validator;

    public GetLocationsHandler(
        IReadDbContext context,
        IValidator<GetLocationsQuery> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result<GetLocationsResponse, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.ToValidationErrors();
        }
        
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