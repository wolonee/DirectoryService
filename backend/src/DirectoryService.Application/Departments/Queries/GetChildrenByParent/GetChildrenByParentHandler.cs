using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Locations.Common;
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
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";

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
        
        var pagination = query.Pagination ?? new PaginationRequest();

        int pageSize = pagination.PageSize;
        int offset = (pagination.Page - 1) * pageSize;

        var parameters = new DynamicParameters();
        parameters.Add(DEPARTMENT_ID, query.ParentId);
        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);

        long? totalCount = null;

        var result = await dbConnection.QueryAsync<GetDepartmentChildrenByParentDto, long, GetDepartmentChildrenByParentDto>(
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

             SELECT c.*,
                    EXISTS(SELECT 1 FROM department WHERE parent_id = c.id) AS has_more_children,
                    COUNT(*) OVER() AS total_count
             FROM children c
             ORDER BY c.name
             LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
             """,
            param: parameters,
            splitOn: "total_count",
            map: (child, count) =>
            {
                if (totalCount == null)
                    totalCount = count;

                return child;
            });

        var count = totalCount ?? 0;
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return new GetDepartmentChildrenByParentResponse(
            result.ToList(),
            count,
            pagination.Page,
            pageSize,
            totalPages);
    }
}