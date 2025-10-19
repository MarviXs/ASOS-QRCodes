using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class ConcurrencyError : Error
{
    public ConcurrencyError()
        : base("Concurrency error") { }
}
