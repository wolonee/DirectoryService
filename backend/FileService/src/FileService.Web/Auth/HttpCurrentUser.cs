using System.Security.Claims;
using FileService.Core.Abstractions;

namespace FileService.Web.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            string? value = httpContextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid userId) ? userId : Guid.Empty;
        }
    }
}
