namespace FileService.Contracts.HttpCommunication;

public sealed record FileServiceOptions
{
    public const string SectionName = "FileService";

    public Uri BaseUrl { get; init; } = null!;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
