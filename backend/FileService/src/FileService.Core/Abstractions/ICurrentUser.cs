namespace FileService.Core.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
}
