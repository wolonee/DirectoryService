using FileService.Domain;
using FileService.Web.EndpointsExtensions;
using FileService.Core.Features.SimpleUpload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpoints(typeof(InitiateUploadEndpoint).Assembly);
        services.AddScoped<IMediaAssetFactory, MediaAssetFactory>();
        services.AddScoped<InitiateUploadHandler>();
        services.AddScoped<CancelUploadHandler>();
        services.AddScoped<CompleteUploadHandler>();
        services.AddScoped<DeleteMediaAssetHandler>();
        services.AddScoped<GetMediaAssetHandler>();
        services.AddScoped<GetMediaAssetsByTargetHandler>();
        services.AddScoped<GetMediaAssetsHandler>();
        
        return services;
    }
}
