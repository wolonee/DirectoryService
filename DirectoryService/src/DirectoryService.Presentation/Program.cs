using DirectoryService.Infrastructure;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddProgramDependencies();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SchemaFilter<EnvelopeSchemaFilter>();
});

services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();