using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public class GetDepartmentChildrenByParentQuery(Guid ParentId) : IQuery;