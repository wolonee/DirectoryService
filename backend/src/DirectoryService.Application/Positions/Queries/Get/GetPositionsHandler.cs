using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Common;
using DirectoryService.Contracts.Positions.Dto;
using DirectoryService.Contracts.Positions.Responses;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Queries.Get;

public class GetPositionsHandler : IQueryHandler<GetPositionsResponse, GetPositionsQuery>
{
    private readonly IValidator<GetPositionsQuery> _validator;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetPositionsHandler> _logger;

    private const string SEARCH_PARAMETER = "search";
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";

    public GetPositionsHandler(
        IValidator<GetPositionsQuery> validator,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetPositionsHandler> logger)
    {
        _validator = validator;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<GetPositionsResponse, Errors>> Handle(
        GetPositionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Get Positions Failed: {Error}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }

        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        var conditions = new List<string> { "p.is_deleted = false" };
        var request = query.Request;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"(p.name->>'Speciality' ILIKE '%' || @{SEARCH_PARAMETER} || '%' OR p.name->>'Direction' ILIKE '%' || @{SEARCH_PARAMETER} || '%')");
            parameters.Add(SEARCH_PARAMETER, request.Search, DbType.String);
        }

        var pagination = request.Pagination ?? new PaginationRequest();

        int pageSize = pagination.PageSize;
        int offset = (pagination.Page - 1) * pageSize;

        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);

        string direction = request.SortDir?.ToLower() == "asc" ? "ASC" : "DESC";

        string orderByField = request.SortBy?.ToLower() switch
        {
            "speciality" => "p.name->>'Speciality'",
            "direction" => "p.name->>'Direction'",
            "created_at" => "p.created_at",
            _ => "p.created_at"
        };

        string whereClause = $"WHERE {string.Join(" AND ", conditions)}";
        string orderByClause = $"ORDER BY {orderByField} {direction}";

        long? totalCount = null;

        var result = await connection.QueryAsync<GetPositionsDto, long, GetPositionsDto>(
            $"""
             SELECT p.id,
                    p.name->>'Speciality' AS speciality,
                    p.name->>'Direction'  AS direction,
                    p.created_at,
                    COUNT(*) OVER() AS total_count

             FROM position p
             {whereClause}
             {orderByClause}
             LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
             """,
            param: parameters,
            splitOn: "total_count",
            map: (pos, count) =>
            {
                if (totalCount == null)
                    totalCount = count;

                return pos;
            });

        var count = totalCount ?? 0;
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return new GetPositionsResponse(
            result.ToList(),
            count,
            pagination.Page,
            pageSize,
            totalPages);
    }
}
