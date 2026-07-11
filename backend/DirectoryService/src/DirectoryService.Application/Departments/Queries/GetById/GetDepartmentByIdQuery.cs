using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetById;

public record GetDepartmentByIdQuery(Guid Id) : IQuery;