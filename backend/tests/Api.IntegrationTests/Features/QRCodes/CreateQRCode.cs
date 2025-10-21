using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Features.QRCodes.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class CreateQRCodeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateQRCode_ShouldReturnCreated()
    {
        // Arrange
        var qrCodeRequest = new CreateDeviceRequestFake().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("qr-codes", qrCodeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        var createdQRCode = await AppDbContext.QRCodes.FindAsync(createdId);
        createdQRCode.Should().NotBeNull();
        createdQRCode!.DisplayName.Should().Be(qrCodeRequest.DisplayName);
        createdQRCode.RedirectUrl.Should().Be(qrCodeRequest.RedirectUrl);
        createdQRCode.ShortCode.Should().Be(qrCodeRequest.ShortCode);
        createdQRCode.DotStyle.Should().Be(qrCodeRequest.DotStyle);
        createdQRCode.CornerDotStyle.Should().Be(qrCodeRequest.CornerDotStyle);
        createdQRCode.CornerSquareStyle.Should().Be(qrCodeRequest.CornerSquareStyle);
        createdQRCode.Color.Should().Be(qrCodeRequest.Color);
    }
}

public class CreateDeviceRequestFake : Faker<CreateQRCode.Request>
{
    public CreateDeviceRequestFake()
    {
        CustomInstantiator(f => new CreateQRCode.Request(
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
