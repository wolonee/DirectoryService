using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetById;

public class GetDepartmentByIdHandler : IQueryHandler<Guid, GetDepartmentByIdQuery>
{
    private readonly IReadDbContext _dbContext;
    private readonly ILogger<GetDepartmentByIdHandler> _logger;

    public GetDepartmentByIdHandler(
        IReadDbContext dbContext,
        ILogger<GetDepartmentByIdHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var departmentId = await _dbContext.DepartmentsRead
            .Where(department => department.Id == query.Id)
            .Select(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (departmentId == Guid.Empty)
        {
            _logger.LogError($"Department with id: {query.Id} not found");
            return GeneralErrors.NotFound(query.Id).ToErrors();
        }

        return departmentId;
    }
}