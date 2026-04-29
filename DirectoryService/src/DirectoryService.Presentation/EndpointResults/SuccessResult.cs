using System.Net;
using DirectoryService.Shared;

namespace DirectoryService.Presentation.EndpointResults;

public sealed class SuccessResult<TValue> : IResult
{
    private readonly TValue _value;

    public SuccessResult(TValue value)
    {
        _value = value;
    }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        
        var envelope = Envelope.Ok(_value);

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        
        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}

public sealed class SuccessResult : IResult
{
    public SuccessResult() { }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        
        var envelope = Envelope.Ok();

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        
        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}