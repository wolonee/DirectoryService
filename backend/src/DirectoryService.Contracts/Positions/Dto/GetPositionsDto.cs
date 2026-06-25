namespace DirectoryService.Contracts.Positions.Dto;

public class GetPositionsDto
{
    public Guid Id { get; set; }

    public string Speciality { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
