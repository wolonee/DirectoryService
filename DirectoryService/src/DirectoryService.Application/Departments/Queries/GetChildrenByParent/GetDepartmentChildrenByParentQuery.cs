using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public record GetDepartmentChildrenByParentQuery(Guid ParentId) : IQuery;