using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TElement, Error>> factoryMethon)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TElement, Error> result = factoryMethon(value);

            if (result.IsSuccess)
                return;

            context.AddFailure(JsonSerializer.Serialize(result.Error));
        });
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithMessage(JsonSerializer.Serialize(error));
    }
}