using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class BadRequestError : Error
{
    public BadRequestError()
        : base("Bad request") { }

    public BadRequestError(string message)
        : base(message) { }
}
