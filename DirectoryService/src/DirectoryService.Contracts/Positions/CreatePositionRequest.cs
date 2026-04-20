namespace DirectoryService.Contracts.Positions;

public record CreatePositionRequest(CreatePositionNameRequest PositionName, string? Description, Guid[] DepartmentIds);