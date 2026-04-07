using DirectoryService.Shared;
using FluentValidation.Results;

namespace DirectoryService.Application.Extentions;

public static class ValidationExtentions
{
    public static Error[] ToErrors(this ValidationResult resultValidation) =>
        resultValidation.Errors.Select(e => Error.Validation(
            e.ErrorCode, 
            e.ErrorMessage, 
            e.PropertyName))
            .ToArray();   
}