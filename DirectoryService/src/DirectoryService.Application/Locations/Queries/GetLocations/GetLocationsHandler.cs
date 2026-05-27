using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _context;
    private readonly IValidator<GetLocationsQuery> _validator;
    private readonly ILogger<GetLocationsHandler> _logger;

    public GetLocationsHandler(
        IReadDbContext context,
        IValidator<GetLocationsQuery> validator,
        ILogger<GetLocationsHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetLocationsResponse, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Get Locations Failed: {Error}", validationResult.ToValidationErrors());
            validationResult.ToValidationErrors();
        }
        
        // filters
        var locationsQuery = _context.LocationsRead;
        
        if (!string.IsNullOrWhiteSpace(query.Request.Search))
            locationsQuery = locationsQuery.Where(l => l.Name.Value.ToLower().Contains(query.Request.Search.ToLower()));
        
        if (query.Request.IsActive == true)
            locationsQuery = locationsQuery.Where(l => l.IsActive == true);
        
        if (query.Request.IsActive == false)
            locationsQuery = locationsQuery.Where(l => l.IsActive == false);
        
        var pagination = query.Request.Pagination ?? new PaginationRequest();
        
        locationsQuery = locationsQuery
            .OrderBy(l => l.CreatedAt);
        
        var totalCount = await locationsQuery.LongCountAsync(cancellationToken);
        
        // pagination
        locationsQuery = locationsQuery
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

        // create response
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

        // return
        return new GetLocationsResponse(result, totalCount);
    }
}