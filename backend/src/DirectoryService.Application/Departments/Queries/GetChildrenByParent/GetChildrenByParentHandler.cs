using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public class GetChildrenByParentHandler : IQueryHandler<GetDepartmentChildrenByParentResponse, GetDepartmentChildrenByParentQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<GetChildrenByParentHandler> _logger;
    
    private const string DEPARTMENT_ID = "department_id";

    public GetChildrenByParentHandler(
        IDbConnectionFactory dbConnectionFactory,
        IDepartmentsRepository departmentsRepository,
        ILogger<GetChildrenByParentHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentChildrenByParentResponse, Errors>> Handle(
        GetDepartmentChildrenByParentQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection dbConnection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var existsParentResult = await _departmentsRepository.Exists(query.ParentId, cancellationToken);
        if (existsParentResult.IsFailure)
            return existsParentResult.Error.ToErrors();

        if (!existsParentResult.Value)
        {
            _logger.LogError($"Parent department not found by id: {query.ParentId}");
            return GeneralErrors.NotFound(query.ParentId, "department").ToErrors();
        }
        
        var parameters = new DynamicParameters();
        parameters.Add(DEPARTMENT_ID, query.ParentId);

        var result = await dbConnection.QueryAsync<GetDepartmentChildrenByParentDto>(
            $"""
             WITH children AS (SELECT d.id,
                                   d.parent_id,
                                   d.name,
                                   d.identifier,
                                   d.path,
                                   d.depth,
                                   d.is_active,
                                   d.created_at,
                                   d.updated_at
                            FROM department d
                            WHERE d.parent_id = @{DEPARTMENT_ID}
                              AND d.is_deleted = false)

             SELECT c.*, EXISTS(SELECT 1 FROM department WHERE parent_id = c.id) AS has_more_children
             FROM children c
             """,
            param: parameters);

        return new GetDepartmentChildrenByParentResponse(result.ToList());
    }
}