using System.Security.Claims;
using FluentResults;

namespace Fei.Is.Api.Extensions;

public static class FluentResultsExtensions
{
    public static string GetError(this Result result)
    {
        return result.Errors[0].Message ?? "Unknown error";
    }

    public static string GetError<T>(this Result<T> result)
    {
        return result.Errors.Count > 0 ? result.Errors[0].Message : "Unknown error";
    }
}
