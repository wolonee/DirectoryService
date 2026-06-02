using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.Get;

public record GetDepartmentsQuery(GetDepartmentsRequest request) : IQuery;