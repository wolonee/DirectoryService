using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.Get;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsResponse, GetDepartmentsQuery>
{
    private readonly IValidator<GetDepartmentsQuery> _validator;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetDepartmentsHandler> _logger;
    
    private const string SEARCH_PARAMETER = "search";
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string LOCATION_IDS_PARAMETER = "location_ids";

    public GetDepartmentsHandler(
        IValidator<GetDepartmentsQuery> validator,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetDepartmentsHandler> logger)
    {
        _validator = validator;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentsResponse, Errors>> Handle(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Get Locations Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        var request = query.Request;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"d.name ILIKE '%' || @{SEARCH_PARAMETER} || '%'");
            parameters.Add(SEARCH_PARAMETER, request.Search, DbType.String);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add($"d.is_active = @{IS_ACTIVE_PARAMETER}");
            parameters.Add(IS_ACTIVE_PARAMETER, request.IsActive, DbType.Boolean);
        }

        if (request.LocationIds != null && request.LocationIds.Length > 0)
        {
            conditions.Add($"EXISTS (SELECT 1 FROM department_locations dl WHERE dl.department_id = d.id AND dl.location_id = ANY(@{LOCATION_IDS_PARAMETER}))");
            parameters.Add(LOCATION_IDS_PARAMETER, request.LocationIds);
        }

        var pagination = request.Pagination ?? new PaginationRequest();
        
        int pageSize = pagination.PageSize;
        int offset = (pagination.Page - 1) * pageSize;
        
        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);
        
        string direction = request.SortDir?.ToLower() == "asc" ? "ASC" : "DESC";

        string orderByField = request.SortBy?.ToLower() switch
        {
            "name" => "name",
            "country" => "country",
            "created_at" => "created_at",
            _ => "name"
        };

        var orderByClause = $"ORDER BY {orderByField} {direction}";
        
        string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" and ", conditions)}" : "";
        
        long? totalCount = null;
        
        var result = await connection.QueryAsync<GetDepartmentsDto, long, GetDepartmentsDto>(
            $"""
             SELECT d.id,
                    d.name,
                    d.path,
                    d.created_at,
                    COUNT(*) OVER() AS total_count

             FROM department d
             {whereClause}
             {orderByClause}
             LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
             """,
            param: parameters,
            splitOn: "total_count",
            map: (dep, count) =>
            {
                if (totalCount == null)
                    totalCount = count;

                return dep;
            });
        
        var count = totalCount ?? 0;
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return new GetDepartmentsResponse(
            result.ToList(),
            count,
            pagination.Page,
            pageSize,
            totalPages);
    }
}
