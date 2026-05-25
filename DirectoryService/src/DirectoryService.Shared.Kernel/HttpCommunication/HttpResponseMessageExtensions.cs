using System.Net.Http.Json;
using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var dataResponse = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return dataResponse?.ErrorList ?? GeneralErrors.Failure("Error while reading response");
        }

        if (dataResponse is null)
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }
        
        if (dataResponse.ErrorList is not null)
        {
            return dataResponse.ErrorList;
        }
        
        if (dataResponse.Result is null)
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }

        return dataResponse.Result!;
    }
}