using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDescendants;

public record GetDepartmentDescendantsQuery(Guid ParentId) : IQuery;
