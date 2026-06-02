using System.Data;
using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _context;
    private readonly IValidator<GetLocationsQuery> _validator;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetLocationsHandler> _logger;

    private const string SEARCH_PARAMETER = "search";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string DEPARTMENT_IDS_PARAMETER = "department_ids";
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";
    
    public GetLocationsHandler(
        IReadDbContext context,
        IValidator<GetLocationsQuery> validator,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetLocationsHandler> logger)
    {
        _context = context;
        _validator = validator;
        _dbConnectionFactory = dbConnectionFactory;
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
            return validationResult.ToValidationErrors();
        }

        var request = query.Request;
        
        // filters
        // var locationsQuery = _context.LocationsRead;
        // var departmentLocationsQuery = _context.DepartmentLocationsRead;
        //
        // if (!string.IsNullOrWhiteSpace(query.Request.Search))
        //     locationsQuery = locationsQuery.Where(l => l.Name.Value.ToLower().Contains(query.Request.Search.ToLower()));
        //
        // if (query.Request.IsActive == true)
        //     locationsQuery = locationsQuery.Where(l => l.IsActive == true);
        //
        // if (query.Request.IsActive == false)
        //     locationsQuery = locationsQuery.Where(l => l.IsActive == false);
        //
        // if (query.Request.DepartmentIds != null && query.Request.DepartmentIds.Length > 0)
        // {
        //     locationsQuery = locationsQuery
        //         .Where(l => departmentLocationsQuery
        //             .Where(dl => query.Request.DepartmentIds.Contains(dl.DepartmentId))
        //             .Any(dl => dl.LocationId == l.Id));
        // }
        //
        // // pagination
        // var pagination = query.Request.Pagination ?? new PaginationRequest();
        //
        // locationsQuery = locationsQuery
        //     .OrderBy(l => l.CreatedAt);
        //
        // var totalCount = await locationsQuery.LongCountAsync(cancellationToken);
        //
        // locationsQuery = locationsQuery
        //     .Skip((pagination.Page - 1) * pagination.PageSize)
        //     .Take(pagination.PageSize);
        //
        // // create response
        // var result = await locationsQuery
        //     .Select(l => new GetLocationDto
        //     {
        //         Id = l.Id,
        //         Name = l.Name.Value,
        //         City = l.Address.City,
        //         Country = l.Address.Country,
        //         Street = l.Address.Street,
        //         Timezone = l.Timezone.Value,
        //     })
        //     .ToListAsync(cancellationToken: cancellationToken);
        //
        // return

        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        var joins = new List<string>();
        
        var pagination = request.Pagination ?? new PaginationRequest();
        
        int pageSize = pagination.PageSize;
        int offset = (pagination.Page - 1) * pageSize;
        
        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"l.name ILIKE '%' || @{SEARCH_PARAMETER} || '%'");
            parameters.Add("search", request.Search, DbType.String);
        }
        
        if (request.IsActive.HasValue)
        {
            conditions.Add($"l.is_active = @{IS_ACTIVE_PARAMETER}");
            parameters.Add("is_active", request.IsActive, DbType.Boolean);
        }

        if (request.DepartmentIds != null && request.DepartmentIds.Length > 0)
        {
            joins.Add("JOIN department_locations dl ON dl.location_id = l.id");
            conditions.Add($"dl.department_id = ANY(@{DEPARTMENT_IDS_PARAMETER})");
            parameters.Add(DEPARTMENT_IDS_PARAMETER, request.DepartmentIds);
        }
        
        var whereClause = conditions.Count > 0 ? $" WHERE {string.Join(" and ", conditions)}" : "";
        
        string direction = request.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

        string orderByField = request.SortBy?.ToLower() switch
        {
            "name" => "name",
            "country" => "country",
            "created_at" => "created_at",
            _ => "name"
        };

        var orderByClause = $"ORDER BY {orderByField} {direction}";
            
        var joinClause = string.Join("\n", joins);

        long? totalCount = null;

        var locations = await connection.QueryAsync<GetLocationDto, long, GetLocationDto>(
            $"""
            SELECT l.id,
                   l.name,
                   l.country,
                   l.city,
                   l.street,
                   l.is_active,
                   l.timezone,
                   l.created_at,
                   COUNT(*) OVER() AS total_count
                   
            FROM locations l
            {joinClause}
            {whereClause}
            {orderByClause}
            LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
            """, 
            splitOn: "total_count",
            map: (loc, count) =>
            {
                totalCount ??= count;
                
                return loc;
            },
            param: parameters);
        
        return new GetLocationsResponse(locations.ToList(), totalCount ?? 0);
    }
}