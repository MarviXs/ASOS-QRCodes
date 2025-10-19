using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class LoginFailedError : Error
{
    public LoginFailedError()
        : base("Login Failed") { }
}
