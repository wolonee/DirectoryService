using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetDepartmentParentsByNameRequest(
    string Name,
    PaginationRequest? Pagination);