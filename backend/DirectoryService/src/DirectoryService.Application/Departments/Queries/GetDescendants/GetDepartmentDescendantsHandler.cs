using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Common;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDescendants;

public class GetDepartmentDescendantsHandler : IQueryHandler<PaginationResponse<GetDepartmentChildrenByParentDto>, GetDepartmentDescendantsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<GetDepartmentDescendantsHandler> _logger;

    private const string DEPARTMENT_ID = "department_id";

    public GetDepartmentDescendantsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IDepartmentsRepository departmentsRepository,
        ILogger<GetDepartmentDescendantsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<PaginationResponse<GetDepartmentChildrenByParentDto>, Errors>> Handle(
        GetDepartmentDescendantsQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection dbConnection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var existsParentResult = await _departmentsRepository.Exists(query.ParentId, cancellationToken);
        if (existsParentResult.IsFailure)
            return existsParentResult.Error.ToErrors();

        if (!existsParentResult.Value)
        {
            _logger.LogError($"Department not found by id: {query.ParentId}");
            return GeneralErrors.NotFound(query.ParentId, "department").ToErrors();
        }

        var parameters = new DynamicParameters();
        parameters.Add(DEPARTMENT_ID, query.ParentId);

        // Все потомки узла по ltree: path <@ (путь узла); сам узел исключён.
        var result = await dbConnection.QueryAsync<GetDepartmentChildrenByParentDto>(
            $"""
             SELECT d.id,
                    d.parent_id,
                    d.name,
                    d.identifier,
                    d.path,
                    d.depth,
                    d.is_active,
                    d.created_at,
                    d.updated_at,
                    EXISTS(SELECT 1 FROM department WHERE parent_id = d.id) AS has_more_children
             FROM department d
             WHERE d.path <@ (SELECT path FROM department WHERE id = @{DEPARTMENT_ID})::ltree
               AND d.id <> @{DEPARTMENT_ID}
               AND d.is_deleted = false
             ORDER BY d.path
             """,
            param: parameters);

        var items = result.ToList();

        return new PaginationResponse<GetDepartmentChildrenByParentDto>(
            items,
            items.Count,
            1,
            items.Count,
            1);
    }
}
