using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class ForbiddenError : Error
{
    public ForbiddenError()
        : base("Not found") { }
}
