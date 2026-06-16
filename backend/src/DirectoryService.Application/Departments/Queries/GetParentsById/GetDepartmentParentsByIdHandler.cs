using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetParentsById;

public class GetDepartmentParentsByIdHandler : IQueryHandler<GetDepartmentParentsByIdResponse, GetDepartmentParentsByIdQuery>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetDepartmentParentsByIdHandler> _logger;

    public GetDepartmentParentsByIdHandler(
        IDepartmentsRepository departmentsRepository,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetDepartmentParentsByIdHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }
    
    private const string DEPARTMENT_PATH = "department_path";

    public async Task<Result<GetDepartmentParentsByIdResponse, Errors>> Handle(
        GetDepartmentParentsByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var departmentResult = await _departmentsRepository.GetByIdAsync(query.Id, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();
        
        var department = departmentResult.Value;

        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        parameters.Add(DEPARTMENT_PATH, department.DepartmentPath.Value);

        var result = await connection.QueryAsync<GetDepartmentParentsByIdDto>(
            $"""
            SELECT d.id,
                  d.parent_id,
                  d.name,
                  d.identifier,
                  d.path,
                  d.depth,
                  d.is_active,
                  d.created_at,
                  d.updated_at
            FROM department d 
            WHERE d.path @> @{DEPARTMENT_PATH}::ltree
                AND d.path != @{DEPARTMENT_PATH}::ltree
            ORDER BY d.depth
            """,
            param: parameters);
        
        return new GetDepartmentParentsByIdResponse(result.ToList());
    }
}