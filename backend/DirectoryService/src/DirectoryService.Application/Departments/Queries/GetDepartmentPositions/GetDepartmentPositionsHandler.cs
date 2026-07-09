using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentPositions;

public class GetDepartmentPositionsHandler : IQueryHandler<PaginationResponse<GetDepartmentPositionsDto>, GetDepartmentPositionsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<GetDepartmentPositionsHandler> _logger;

    private const string DEPARTMENT_ID = "department_id";
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";

    public GetDepartmentPositionsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IDepartmentsRepository departmentsRepository,
        ILogger<GetDepartmentPositionsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<PaginationResponse<GetDepartmentPositionsDto>, Errors>> Handle(
        GetDepartmentPositionsQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection dbConnection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var existsDepartmentResult = await _departmentsRepository.Exists(query.DepartmentId, cancellationToken);
        if (existsDepartmentResult.IsFailure)
            return existsDepartmentResult.Error.ToErrors();

        if (!existsDepartmentResult.Value)
        {
            _logger.LogError($"Department not found by id: {query.DepartmentId}");
            return GeneralErrors.NotFound(query.DepartmentId, "department").ToErrors();
        }

        var pagination = query.Pagination ?? new PaginationRequest();

        int pageSize = pagination.PageSize;
        int offset = (pagination.Page - 1) * pageSize;

        var parameters = new DynamicParameters();
        parameters.Add(DEPARTMENT_ID, query.DepartmentId);
        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);

        long? totalCount = null;

        var result = await dbConnection.QueryAsync<GetDepartmentPositionsDto, long, GetDepartmentPositionsDto>(
            $"""
             SELECT p.id,
                    p.name->>'Speciality' AS speciality,
                    p.name->>'Direction'  AS direction,
                    p.is_active,
                    p.created_at,
                    COUNT(*) OVER() AS total_count
             FROM department_positions dp
             JOIN position p ON p.id = dp.position_id
             WHERE dp.department_id = @{DEPARTMENT_ID}
               AND p.is_deleted = false
             ORDER BY p.name->>'Speciality'
             LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
             """,
            param: parameters,
            splitOn: "total_count",
            map: (position, count) =>
            {
                if (totalCount == null)
                    totalCount = count;

                return position;
            });

        var count = totalCount ?? 0;
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return new PaginationResponse<GetDepartmentPositionsDto>(
            result.ToList(),
            count,
            pagination.Page,
            pageSize,
            totalPages);
    }
}
