using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Errors.Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }
    }
    
    public static async Task<UnitResult<Errors.Errors>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dataResponse = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

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

            return UnitResult.Success<Errors.Errors>();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }
    }
}