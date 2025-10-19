using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class AlreadyExistError : Error
{
    public AlreadyExistError()
        : base("Already exist") { }

    public AlreadyExistError(string message)
        : base(message) { }
}
