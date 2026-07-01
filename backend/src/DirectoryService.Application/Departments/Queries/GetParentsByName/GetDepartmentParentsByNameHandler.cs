using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Common;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentParentsByName;

public class GetDepartmentParentsByNameHandler : IQueryHandler<PaginationResponse<GetDepartmentParentsByNameWithParentsDto>, GetDepartmentParentsByNameQuery>
{
    private readonly IValidator<GetDepartmentParentsByNameQuery> _validator;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetDepartmentParentsByNameHandler> _logger;

    public GetDepartmentParentsByNameHandler(
        IValidator<GetDepartmentParentsByNameQuery> validator,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetDepartmentParentsByNameHandler> logger)
    {
        _validator = validator;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }
    
    private const string SEARCH_PARAMETER = "search_parameter";
    private const string LIMIT = "limit";
    private const string OFFSET = "offset";

    public async Task<Result<PaginationResponse<GetDepartmentParentsByNameWithParentsDto>, Errors>> Handle(
        GetDepartmentParentsByNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed: {errors}", validationResult.ToValidationErrors());
            return validationResult.ToValidationErrors();
        }
        
        var request = query.Request;
        var pagination = request.Pagination ?? new PaginationRequest();
        
        string searchName = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : "";
        
        var parameters = new DynamicParameters();
        parameters.Add(SEARCH_PARAMETER, searchName);
        parameters.Add(LIMIT, pagination.PageSize);
        parameters.Add(OFFSET, (pagination.Page - 1) * pagination.PageSize);
        
        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        long? totalCount = null;
        
        var result = await connection.QueryAsync<GetDepartmentParentsByNameForSqlDto, long, GetDepartmentParentsByNameForSqlDto>(
            $"""
            WITH found_departments AS (
                                    SELECT d.id,
                                          d.parent_id,
                                          d.name,
                                          d.identifier,
                                          d.path,
                                          d.depth,
                                          d.is_active,
                                          d.created_at,
                                          d.updated_at,
                                          COUNT(*) OVER() as count_found_departments
                                    FROM department d 
                                    WHERE d.name ILIKE '%' || @{SEARCH_PARAMETER} || '%'
                                    AND d.is_deleted = false
                                    LIMIT @limit OFFSET @offset
                                    ),
                         ancestors AS (
                                    SELECT
                                        fd.id AS found_department_id,
                                        
                                        a.id AS ancestor_id,
                                        a.parent_id AS ancestor_parent_id,
                                        a.name AS ancestor_name,
                                        a.identifier AS ancestor_identifier,
                                        a.path AS ancestor_path,
                                        a.depth AS ancestor_depth,
                                        a.is_active AS ancestor_is_active,
                                        a.created_at AS ancestor_created_at,
                                        a.updated_at AS ancestor_updated_at
                                           
                                    FROM found_departments fd
                                    CROSS JOIN LATERAL (
                                        SELECT *
                                        FROM department d
                                        WHERE d.path @> fd.path
                                            AND d.path != fd.path
                                            AND d.is_deleted = false
                                        ) AS a
                                    )
            SELECT        
                anc.ancestor_id,
                anc.ancestor_parent_id,
                anc.ancestor_name,
                anc.ancestor_identifier,
                anc.ancestor_path,
                anc.ancestor_depth,
                anc.ancestor_is_active,
                anc.ancestor_created_at,
                anc.ancestor_updated_at,
                fd.*
            FROM found_departments fd
            JOIN ancestors anc ON anc.found_department_id = fd.id
            """,
            param: parameters,
            splitOn: "count_found_departments",
            map: (dep, count) =>
            {
                if (totalCount is null)
                {
                    totalCount = count;
                }

                return dep;
            });

        var lookup = result
            .GroupBy(r => r.Id)
            .Select(g => new GetDepartmentParentsByNameWithParentsDto
            {
                Id = g.First().Id,
                ParentId = g.First().ParentId,
                Name = g.First().Name,
                Identifier = g.First().Identifier,
                Path = g.First().Path,
                Depth = g.First().Depth,
                IsActive = g.First().IsActive,
                CreatedAt = g.First().CreatedAt,
                UpdatedAt = g.First().UpdatedAt,
                Parents = g.Select(p => new GetDepartmentParentsByNameDto
                {
                    Id = p.AncestorId,
                    ParentId = p.AncestorParentId,
                    Name = p.AncestorName,
                    Identifier = p.AncestorIdentifier,
                    Path = p.AncestorPath,
                    Depth = p.AncestorDepth,
                    IsActive = p.AncestorIsActive,
                    CreatedAt = p.AncestorCreatedAt,
                    UpdatedAt = p.AncestorUpdatedAt,
                })
                    .ToList(),
            })
            .ToList();

        var count = totalCount ?? 0;
        var totalPages = (int)Math.Ceiling((double)count / pagination.PageSize);

        return new PaginationResponse<GetDepartmentParentsByNameWithParentsDto>(
            lookup,
            count,
            pagination.Page,
            pagination.PageSize,
            totalPages);
    }
}