using System.Security.Claims;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.QRCodes.Extensions;

namespace Fei.Is.Api.UnitTests.Extensions;

public class QRCodeExtensionsTests
{
    [Theory]
    [InlineData("example.com/path", "https://example.com/path")]
    [InlineData(" http://already.secure ", "http://already.secure")]
    [InlineData("https://secure.site", "https://secure.site")]
    public void EnsureHttpsScheme_NormalizesRedirectUrl(string input, string expected)
    {
        var result = input.EnsureHttpsScheme();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsOwner_ReturnsTrue_WhenUserOwnsQrCode()
    {
        var ownerId = Guid.NewGuid();
        var qrCode = new QRCode
        {
            OwnerId = ownerId,
            DisplayName = "Test",
            RedirectUrl = "https://example.com",
            ShortCode = "abc",
            DotStyle = "square",
            CornerDotStyle = "square",
            CornerSquareStyle = "square",
            Color = "#000000"
        };

        var user = TestUtils.CreateUserPrincipal(ownerId);

        Assert.True(qrCode.IsOwner(user));
    }

    [Fact]
    public void IsOwner_ReturnsFalse_WhenUserDoesNotOwnQrCode()
    {
        var qrCode = new QRCode
        {
            OwnerId = Guid.NewGuid(),
            DisplayName = "Test",
            RedirectUrl = "https://example.com",
            ShortCode = "abc",
            DotStyle = "square",
            CornerDotStyle = "square",
            CornerSquareStyle = "square",
            Color = "#000000"
        };

        var user = TestUtils.CreateUserPrincipal(Guid.NewGuid());

        Assert.False(qrCode.IsOwner(user));
    }
}
