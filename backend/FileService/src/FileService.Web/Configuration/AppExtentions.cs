using CrystalQuartz.AspNetCore;
using DirectoryService.Presentation.Middlewares;
using FileService.Web.EndpointsExtensions;
using FileService.Web.Progress;
using Quartz;
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
        app.UseCrystalQuartz(() => app.Services.GetRequiredService<ISchedulerFactory>().GetScheduler());
        app.MapEndpoints();

        // SignalR-хаб прогресса: клиент подключается сюда (через nginx — /api/hubs/progress).
        app.MapHub<ProgressHub>("/hubs/progress");

        return app;
    }
}
