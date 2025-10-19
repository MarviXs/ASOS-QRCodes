using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class UserAlreadyExistsError : Error
{
    public UserAlreadyExistsError()
        : base("User already exists") { }
}
