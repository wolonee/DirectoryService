using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetRoots;

public class GetDepartmentsRootsHandler : IQueryHandler<PaginationResponse<GetDepartmentRootsDto>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetDepartmentsRootsHandler> _logger;

    public GetDepartmentsRootsHandler(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetDepartmentsRootsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<PaginationResponse<GetDepartmentRootsDto>, Errors>> Handle(CancellationToken cancellationToken = default)
    {
        IDbConnection dbConnection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var result = await dbConnection.QueryAsync<GetDepartmentRootsDto>(
            $"""
            WITH roots AS (SELECT d.id,
                                  d.parent_id,
                                  d.name,
                                  d.identifier,
                                  d.path,
                                  d.depth,
                                  d.is_active,
                                  d.created_at,
                                  d.updated_at
                           FROM department d
                           WHERE d.parent_id IS NULL
                             AND d.is_deleted = false)
            
            SELECT r.*, EXISTS(SELECT 1 FROM department c WHERE c.parent_id = r.id) AS has_more_children
            FROM roots r
            """);

        var items = result.ToList();

        return new PaginationResponse<GetDepartmentRootsDto>(items, items.Count, 1, items.Count, 1);
    }
}