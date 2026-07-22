using DirectoryService.Presentation.Middlewares;
using FileService.Web.EndpointsExtensions;
using Serilog;

namespace FileService.Web.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service V1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.MapHealthChecks("/health");
        app.MapEndpoints();

        return app;
    }
}
