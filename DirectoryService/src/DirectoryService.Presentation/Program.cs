using System.Globalization;
using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Seeder;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Extentions;
using DirectoryService.Presentation.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var services = builder.Services;

    services.AddProgramDependencies(builder.Configuration);

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SchemaFilter<EnvelopeSchemaFilter>();
    });

    services.AddScoped<DirectoryServiceDbContext>(_ =>
        new DirectoryServiceDbContext(builder.Configuration.GetConnectionString(NameConstants.DATABASE)!));

    var app = builder.Build();

    app.UseExceptionMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = "swagger"; // Можно изменить на string.Empty для корневого URL
        });
    }
    
    // using (var scope = app.Services.CreateScope())
    // {
    //     var context = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
    //     
    //     var seeder = new DepartmentTreeSeeder(context);
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}