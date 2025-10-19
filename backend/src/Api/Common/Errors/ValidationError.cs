using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Common.Errors;

public class ValidationError : Error
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    public ValidationError(Dictionary<string, string[]> errors)
        : base("Validation error")
    {
        ValidationErrors = errors;
    }

    public ValidationError(string key, string message)
        : base("Validation error")
    {
        ValidationErrors = new Dictionary<string, string[]> { { key, new[] { message } } };
    }

    public ValidationError(ValidationResult validationResult)
        : base("Validation error")
    {
        ValidationErrors = validationResult.ToDictionary();
    }

    public ValidationError(IdentityResult identityResult)
        : base("Validation error")
    {
        var errors = identityResult.Errors.GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.Select(y => y.Description).ToArray());
        ValidationErrors = errors;
    }
}
