using System.Globalization;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Seeder;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Extentions;
using DirectoryService.Presentation.Middlewares;
using DirectoryService.Shared.Serializations;
using Serilog;

var isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";

// В режиме тестирования не создаем статический логгер, чтобы избежать конфликта с WebApplicationFactory
if (!isTesting)
{
    Log.CloseAndFlush();
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .CreateLogger();
}


var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddProgramDependencies(builder.Configuration);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{   
    options.SchemaFilter<EnvelopeSchemaFilter>();
});

services.Configure<CleanupServiceOptions>(
    builder.Configuration.GetSection(nameof(CleanupServiceOptions)));

services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString(NameConstants.DATABASE)!));

services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString(NameConstants.DATABASE)!));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new ErrorsJsonConverter());
});

var app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Можно изменить на string.Empty для корневого URL
    });
}

// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
//     
//     var seeder = new DepartmentTreeSeeder(context);
//     
//     Console.WriteLine("Clear database...");
//
//     await seeder.ClearAsync();
//
//     Console.WriteLine("Starting seeding...");
//     
//     var count = await seeder.SeedAsync();
//     Console.WriteLine($"Seeded {count} departments.");
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


namespace DirectoryService.Presentation
{
    public partial class Program;
}