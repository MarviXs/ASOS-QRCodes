using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class NotFoundError : Error
{
    public NotFoundError()
        : base("Not found") { }
}
