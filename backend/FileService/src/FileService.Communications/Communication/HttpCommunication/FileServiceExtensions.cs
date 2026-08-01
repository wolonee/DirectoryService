using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Contracts.HttpCommunication;

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileServiceHttpCommunication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<FileServiceOptions>, FileServiceOptionsValidator>();

        services
            .AddOptions<FileServiceOptions>()
            .Bind(configuration.GetSection(FileServiceOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpClient<IFileCommunicationService, FileCommunicationService>((serviceProvider, client) =>
        {
            FileServiceOptions options = serviceProvider
                .GetRequiredService<IOptions<FileServiceOptions>>()
                .Value;

            client.BaseAddress = options.BaseUrl;
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
