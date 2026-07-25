using System.Security.Claims;
using FileService.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FileService.Web.Auth;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IHostEnvironment environment) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            string? value = httpContextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (Guid.TryParse(value, out Guid userId))
                return userId;

            if ((environment.IsDevelopment() || environment.EnvironmentName.Equals("Docker", StringComparison.OrdinalIgnoreCase))
                && Guid.TryParse(configuration["Development:MockUserId"], out Guid mockUserId))
            {
                return mockUserId;
            }

            return Guid.Empty;
        }
    }
}
