using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.GetParentsByName;

public record GetDepartmentParentsByNameQuery(GetDepartmentParentsByNameRequest Request) : IQuery;