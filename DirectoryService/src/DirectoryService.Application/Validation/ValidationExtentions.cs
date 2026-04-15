using System.Text.Json;
using DirectoryService.Shared;
using FluentValidation.Results;

namespace DirectoryService.Application.Validation;

public static class ValidationExtentions
{
    // public static Errors ToValidationErrors(this ValidationResult resultValidation) =>
    //     resultValidation.Errors.Select(e => Error.Validation(
    //         e.ErrorCode, 
    //         e.ErrorMessage, 
    //         e.PropertyName))
    //         .ToArray();   

    public static Errors ToValidationErrors(this ValidationResult resultValidation)
    {
        var errors = resultValidation.Errors.Select(e
            => JsonSerializer.Deserialize<Error>(e.ToString())).ToArray()!;

        return errors!;
    }
}