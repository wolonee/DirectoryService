using System.Text.Json;
using DirectoryService.Application.Exceptions;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);
        
        (int code, Errors? errors) = exception switch
        {
            BadRequestException => (
                StatusCodes.Status400BadRequest, JsonSerializer.Deserialize<Errors>(exception.Message)),

            NotFoundException => (
                StatusCodes.Status404NotFound, JsonSerializer.Deserialize<Errors>(exception.Message)),

            _ => (StatusCodes.Status500InternalServerError, Error.Failure(null, "Something went wrong.")),
        };

        var envelope = Envelope.Errors(errors);
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;

        await context.Response.WriteAsJsonAsync(envelope);
    }
}

public static class ExceptionMiddlewareExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this WebApplication app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}