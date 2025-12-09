using System.Security.Claims;
using Fei.Is.Api.Extensions;

namespace Fei.Is.Api.UnitTests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ReturnsGuid_WhenClaimPresent()
    {
        var userId = Guid.NewGuid();
        var principal = TestUtils.CreateUserPrincipal(userId);

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_Throws_WhenClaimMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_Throws_WhenClaimIsInvalidGuid()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") });
        var principal = new ClaimsPrincipal(identity);

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }
}
