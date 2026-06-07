using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentParentsByName;

public record GetDepartmentParentsByNameQuery(GetDepartmentParentsByNameRequest Request) : IQuery;