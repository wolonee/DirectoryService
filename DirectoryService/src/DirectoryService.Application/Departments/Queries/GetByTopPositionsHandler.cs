using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries;

public class GetByTopPositionsHandler : IQueryHandler<GetTopDepartmentsByPositionsResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetByTopPositionsHandler> _logger;

    public GetByTopPositionsHandler(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetByTopPositionsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<GetTopDepartmentsByPositionsResponse, Errors>> Handle(
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        //language=sql
        var result = await connection.QueryAsync<GetTopDepartmentsDepartmentDto>(
            """
            WITH position_counts AS ( SELECT department_id, COUNT(*) as count_positions
                FROM department_positions
                GROUP BY department_id
            )
            
            SELECT d.id,
                   d.name, 
                   d.path,
                   d.depth,
                   d.is_active,
                   d.created_at,
                   d.updated_at,
                   pc.count_positions
                
            FROM department as d
            JOIN position_counts as pc ON pc.department_id = d.id
            ORDER BY count_positions DESC 
            LIMIT 5 OFFSET 0
            """);
        
        return new GetTopDepartmentsByPositionsResponse(result.ToList());
    }
}