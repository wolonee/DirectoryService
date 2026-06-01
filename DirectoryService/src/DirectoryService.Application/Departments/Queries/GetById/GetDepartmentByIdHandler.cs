using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.Errors;
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

    public Task<Result<Guid, Errors>> Handle(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
    }
}