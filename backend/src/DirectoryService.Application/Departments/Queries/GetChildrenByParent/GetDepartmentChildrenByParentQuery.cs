using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Common;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public record GetDepartmentChildrenByParentQuery(
    Guid ParentId,
    PaginationRequest? Pagination = null) : IQuery;