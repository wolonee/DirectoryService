using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries;

public class GetByTopPositionsHandler : IQueryHandler<PaginationResponse<GetTopDepartmentsDepartmentDto>>
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

    public async Task<Result<PaginationResponse<GetTopDepartmentsDepartmentDto>, Errors>> Handle(
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

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
        //     ORDER BY count_positions DESC, name ASC
        //     LIMIT 5 OFFSET 0
        //     """);
        //
        // return new GetTopDepartmentsByPositionsResponse(result.ToList());
        
        var SqlResult = await connection.QueryAsync<TempDepartmentRow>(
            """
            WITH position_counts AS ( SELECT department_id, COUNT(*) as count_positions
                                      FROM department_positions
                                      GROUP BY department_id
            ),
                 top_departments AS (SELECT d.id,
                                            d.name,
                                            d.path,
                                            d.depth,
                                            d.is_active,
                                            d.created_at,
                                            d.updated_at,
                                            COALESCE(pc.count_positions, 0) as count_positions
            
                                     FROM department as d
                                     LEFT JOIN position_counts AS pc ON pc.department_id = d.id
                                     ORDER BY count_positions DESC, name
                                     LIMIT 5 OFFSET 0)
            
            SELECT td.id as DepartmentId,
                   td.name as DepartmentName,
                   td.path as DepartmentPath,
                   td.depth as DepartmentDepth,
                   td.is_active as DepartmentIsActive,
                   td.created_at as DepartmentCreatedAt,
                   td.updated_at as DepartmentUpdatedAt,
                   td.count_positions as DepartmentCountPositions,
                   p.id as PositionId,
                   p.name->>'Speciality' as PositionSpeciality,
                   p.name->>'Direction' as PositionDirection,
                   p.is_active as PositionIsActive
            
            FROM top_departments td
            JOIN department_positions AS dp ON dp.department_id = td.id
            JOIN position AS p ON p.id = dp.position_id
            """);

        var result = SqlResult
            .GroupBy(r => r.DepartmentId)
            .Select(g => new GetTopDepartmentsDepartmentDto
            {
                Id = g.First().DepartmentId,
                Name = g.First().DepartmentName,
                Path = g.First().DepartmentPath,
                Depth = g.First().DepartmentDepth,
                IsActive = g.First().DepartmentIsActive,
                CreatedAt = g.First().DepartmentCreatedAt,
                UpdatedAt = g.First().DepartmentUpdatedAt,
                CountPositions = g.First().DepartmentCountPositions,
                Positions = g.Select(p => new GetTopDepartmentsDepartmentPositionDto
                {
                    Id = p.PositionId,
                    Speciality = p.PositionSpeciality,
                    Direction = p.PositionDirection,
                    IsActive = p.PositionIsActive,
                })
                    .ToList(),
            })
            .ToList();
        
        return new PaginationResponse<GetTopDepartmentsDepartmentDto>(result, result.Count, 1, result.Count, 1);

        // var deparmentsQuery = _dbContext.DepartmentsRead;
        // var deparmentPositionsQuery = _dbContext.DepartmentPositionsRead;
        // var positionsQuery = _dbContext.PositionsRead;
        //
        // var result = await deparmentsQuery
        //     .Select(d => new
        //     {
        //         Department = d,
        //         CountPositions = deparmentPositionsQuery.Count(dp => dp.DepartmentId == d.Id),
        //         Positions = deparmentPositionsQuery
        //             .Where(dp => dp.DepartmentId == d.Id)
        //             .Join(
        //                 positionsQuery,
        //                 dp => dp.PositionId,
        //                 p => p.Id,
        //                 (dp, p) => new GetTopDepartmentsDepartmentPositionDto
        //                 {
        //                     Id = p.Id,
        //                     Speciality = p.Name.Speciality,
        //                     Direction = p.Name.Direction,
        //                     IsActive = p.IsActive,
        //                 })
        //             .ToList(),
        //     })
        //     .OrderByDescending(x => x.CountPositions)
        //     .Take(5)
        //     .Select(x => new GetTopDepartmentsDepartmentDto()
        //     {
        //         Id = x.Department.Id,
        //         Name = x.Department.DepartmentName.Value,
        //         Path = x.Department.DepartmentPath.Value,
        //         Depth = x.Department.Depth,
        //         IsActive = x.Department.IsActive,
        //         CreatedAt = x.Department.CreatedAt,
        //         UpdatedAt = x.Department.UpdatedAt,
        //         CountPositions = x.CountPositions,
        //         Positions = x.Positions,
        //     })
        //     .ToListAsync(cancellationToken: cancellationToken);

        // return new GetTopDepartmentsByPositionsResponse(result);
    }
}

internal class TempDepartmentRow
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentPath { get; set; } = string.Empty;
    public int DepartmentDepth { get; set; }
    public bool DepartmentIsActive { get; set; }
    public DateTime DepartmentCreatedAt { get; set; }
    public DateTime DepartmentUpdatedAt { get; set; }
    public int DepartmentCountPositions { get; set; }
    public Guid PositionId { get; set; }
    public string PositionSpeciality { get; set; } = string.Empty;
    public string PositionDirection { get; set; } = string.Empty;
    public bool PositionIsActive { get; set; }
}