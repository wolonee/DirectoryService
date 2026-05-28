namespace DirectoryService.Contracts.Positions.Requests;

public record CreatePositionRequest(CreatePositionNameRequest PositionName, string? Description, Guid[] DepartmentIds);