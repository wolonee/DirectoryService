using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries;

public class GetByTopPositionsHandler : IQueryHandler<GetTopDepartmentsByPositionsResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IReadDbContext _dbContext;
    private readonly ILogger<GetByTopPositionsHandler> _logger;

    public GetByTopPositionsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IReadDbContext dbContext,
        ILogger<GetByTopPositionsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<GetTopDepartmentsByPositionsResponse, Errors>> Handle(
        CancellationToken cancellationToken = default)
    {
        // var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        //language=sql
        // var result = await connection.QueryAsync<GetTopDepartmentsDepartmentDto>(
        //     """
        //     WITH position_counts AS ( SELECT department_id, COUNT(*) as count_positions
        //         FROM department_positions
        //         GROUP BY department_id
        //     )
        //     
        //     SELECT d.id,
        //            d.name, 
        //            d.path,
        //            d.depth,
        //            d.is_active,
        //            d.created_at,
        //            d.updated_at,
        //            pc.count_positions
        //         
        //     FROM department as d
        //     JOIN position_counts as pc ON pc.department_id = d.id
        //     ORDER BY count_positions DESC 
        //     LIMIT 5 OFFSET 0
        //     """);
        //
        // return new GetTopDepartmentsByPositionsResponse(result.ToList());

        var deparmentsQuery = _dbContext.DepartmentsRead;
        var deparmentPositionsQuery = _dbContext.DepartmentPositionsRead;
        var positionsQuery = _dbContext.PositionsRead;

        var result = await deparmentsQuery
            .Select(d => new
            {
                Department = d,
                CountPositions = deparmentPositionsQuery.Count(dp => dp.DepartmentId == d.Id),
                Positions = deparmentPositionsQuery
                    .Where(dp => dp.DepartmentId == d.Id)
                    .Join(
                        positionsQuery,
                        dp => dp.PositionId,
                        p => p.Id,
                        (dp, p) => new GetTopDepartmentsDepartmentPositionDto
                        {
                            Id = p.Id,
                            Speciality = p.Name.Speciality,
                            Direction = p.Name.Direction,
                            IsActive = p.IsActive,
                        })
                    .ToList(),
            })
            .OrderByDescending(x => x.CountPositions)
            .Take(5)
            .Select(x => new GetTopDepartmentsDepartmentDto()
            {
                Id = x.Department.Id,
                Name = x.Department.DepartmentName.Value,
                Path = x.Department.DepartmentPath.Value,
                Depth = x.Department.Depth,
                IsActive = x.Department.IsActive,
                CreatedAt = x.Department.CreatedAt,
                UpdatedAt = x.Department.UpdatedAt,
                CountPositions = x.CountPositions,
                Positions = x.Positions,
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetTopDepartmentsByPositionsResponse(result);
    }
}