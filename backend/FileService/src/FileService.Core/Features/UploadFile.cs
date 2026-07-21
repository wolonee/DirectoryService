using FileService.Web.EndpointsExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features;

public class UploadFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files", async Task (
            IFormFile formFile,
            [FromServices] IS3Provider s3Provider,
            CancellationToken cancellationToken) =>
        {
            string key = $"raw/{Guid.NewGuid()}";

            await s3Provider.UploadFileAsync(
                formFile.OpenReadStream(),
                "pictures",
                key,
                formFile.ContentType,
                cancellationToken);
        }).DisableAntiforgery();
    }
}
