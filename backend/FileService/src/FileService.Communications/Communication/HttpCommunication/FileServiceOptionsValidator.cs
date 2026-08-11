using Microsoft.Extensions.Options;

namespace FileService.Communications.Communication.HttpCommunication;

public sealed class FileServiceOptionsValidator : IValidateOptions<FileServiceOptions>
{
    public ValidateOptionsResult Validate(string? name, FileServiceOptions options)
    {
        var failures = new List<string>();

        if (options.BaseUrl is null || !options.BaseUrl.IsAbsoluteUri)
        {
            failures.Add("FileService:BaseUrl must be an absolute URI.");
        }
        else if (options.BaseUrl.Scheme != Uri.UriSchemeHttp
                 && options.BaseUrl.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add("FileService:BaseUrl must use HTTP or HTTPS.");
        }

        if (options.Timeout <= TimeSpan.Zero)
            failures.Add("FileService:Timeout must be greater than zero.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
