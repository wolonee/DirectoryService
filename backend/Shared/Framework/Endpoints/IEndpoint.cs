using Microsoft.AspNetCore.Routing;

namespace FileService.Web.EndpointsExtensions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
