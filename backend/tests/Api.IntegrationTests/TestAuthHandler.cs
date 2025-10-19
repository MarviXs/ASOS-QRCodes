using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fei.Is.Api.IntegrationTests;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public Guid DefaultUserId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
}

public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly Guid _defaultUserId;
    public const string TestScheme = "TestScheme";

    public TestAuthHandler(IOptionsMonitor<TestAuthHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _defaultUserId = options.CurrentValue.DefaultUserId;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "test@test.com"), new(ClaimTypes.Role, "Admin") };

        if (Context.Request.Headers.TryGetValue(UserId.ToString(), out var userId) && userId.Count > 0)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId[0]));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _defaultUserId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
