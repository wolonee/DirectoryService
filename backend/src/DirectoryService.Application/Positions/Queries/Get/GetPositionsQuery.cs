using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Queries.Get;

public record GetPositionsQuery(GetPositionsRequest Request) : IQuery;
