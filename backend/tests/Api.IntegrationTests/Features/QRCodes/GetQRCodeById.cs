using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.QRCodes.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.QRCodes;

[Collection("IntegrationTests")]
public class GetQRCodeByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetQRCodeById_ShouldReturnQRCode()
    {
        // Arrange
        var qrCode = new QRCodeFake(factory.DefaultUserId).Generate();

        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(qrCode).State = EntityState.Detached;

        // Act
        var response = await Client.GetAsync($"qr-codes/{qrCode.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var qrCodeResponse = await response.Content.ReadFromJsonAsync<GetQRCodeById.Response>();
        qrCodeResponse.Should().NotBeNull();
        qrCodeResponse!.Id.Should().Be(qrCode.Id);
        qrCodeResponse.DisplayName.Should().Be(qrCode.DisplayName);
        qrCodeResponse.RedirectUrl.Should().Be(qrCode.RedirectUrl);
        qrCodeResponse.ShortCode.Should().Be(qrCode.ShortCode);
        qrCodeResponse.DotStyle.Should().Be(qrCode.DotStyle);
        qrCodeResponse.CornerDotStyle.Should().Be(qrCode.CornerDotStyle);
        qrCodeResponse.CornerSquareStyle.Should().Be(qrCode.CornerSquareStyle);
        qrCodeResponse.Color.Should().Be(qrCode.Color);
    }

    [Fact]
    public async Task GetQRCodeById_ShouldReturnNotFound_WhenQRCodeMissing()
    {
        // Act
        var response = await Client.GetAsync($"qr-codes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQRCodeById_ShouldReturnForbid_WhenNotOwner()
    {
        // Arrange
        var qrCode = new QRCodeFake(factory.OtherUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"qr-codes/{qrCode.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
