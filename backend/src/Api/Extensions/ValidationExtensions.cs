using FluentResults;
using Fei.Is.Api.Common.Errors;

namespace Fei.Is.Api.Extensions;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToValidationErrors<T>(this Result<T> validationResult)
    {
        IDictionary<string, string[]> errors = new Dictionary<string, string[]>();
        foreach (var error in validationResult.Errors)
        {
            if (error is ValidationError validationFailure)
            {
                errors = validationFailure.ValidationErrors;
            }
        }

        return errors;
    }

    public static IDictionary<string, string[]> ToValidationErrors(this Result validationResult)
    {
        IDictionary<string, string[]> errors = new Dictionary<string, string[]>();
        foreach (var error in validationResult.Errors)
        {
            if (error is ValidationError validationFailure)
            {
                errors = validationFailure.ValidationErrors;
            }
        }

        return errors;
    }
}
