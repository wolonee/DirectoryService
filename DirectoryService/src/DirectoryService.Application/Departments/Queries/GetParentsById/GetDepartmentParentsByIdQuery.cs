using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetParentsById;

public record GetDepartmentParentsByIdQuery(Guid Id) : IQuery;