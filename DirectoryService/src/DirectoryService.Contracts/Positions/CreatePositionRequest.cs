namespace DirectoryService.Contracts.Positions;

public record CreatePositionRequest(CreatePositionNameRequest Name, string? Description, Guid[] DepartmentIds);