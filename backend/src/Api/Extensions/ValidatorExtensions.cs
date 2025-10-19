using Fei.Is.Api.Common.Errors;
using FluentResults;
using FluentValidation;

namespace Fei.Is.Api.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string?> ValidSortBy<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        IEnumerable<string> validSortByFields
    )
    {
        return ruleBuilder
            .Must(field =>
                string.IsNullOrEmpty(field)
                || validSortByFields.Any(validField => string.Equals(validField, field, StringComparison.OrdinalIgnoreCase))
            )
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", validSortByFields)}");
    }
}
