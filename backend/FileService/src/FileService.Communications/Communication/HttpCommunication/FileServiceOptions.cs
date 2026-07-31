namespace FileService.Contracts.HttpCommunication;

public record FileServiceOptions
{
    public string Url { get; init; } = string.Empty;

    public int Timeout { get; init; } = 5; // sec
}