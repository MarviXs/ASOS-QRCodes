using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.QRCodes.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class UpdateQRCodeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateCommand_ShouldReturnNoContent()
    {
        // Arrange
        var qrCode = new QRCodeFake(factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(qrCode).State = EntityState.Detached;
        var updateQrCodeRequest = new UpdateQRCodeRequestFake().Generate();

        // Act
        var response = await Client.PutAsJsonAsync($"qr-codes/{qrCode.Id}", updateQrCodeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedQRCode = await AppDbContext.QRCodes.FindAsync(qrCode.Id);
        updatedQRCode.Should().NotBeNull();
        updatedQRCode!.DisplayName.Should().Be(updateQrCodeRequest.DisplayName);
        updatedQRCode.RedirectUrl.Should().Be(updateQrCodeRequest.RedirectUrl);
        updatedQRCode.ShortCode.Should().Be(updateQrCodeRequest.ShortCode);
        updatedQRCode.DotStyle.Should().Be(updateQrCodeRequest.DotStyle);
        updatedQRCode.CornerDotStyle.Should().Be(updateQrCodeRequest.CornerDotStyle);
        updatedQRCode.CornerSquareStyle.Should().Be(updateQrCodeRequest.CornerSquareStyle);
        updatedQRCode.Color.Should().Be(updateQrCodeRequest.Color);
    }
}

public class UpdateQRCodeRequestFake : Faker<UpdateQRCode.Request>
{
    public UpdateQRCodeRequestFake()
    {
        CustomInstantiator(f => new UpdateQRCode.Request(
            f.Lorem.Word(),
            f.Internet.Url(),
            f.Random.AlphaNumeric(8),
            "square",
            "square",
            "square",
            f.Internet.Color()
        ));
    }
}
