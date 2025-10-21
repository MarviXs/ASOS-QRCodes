using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.QRCodes.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class GetQRCodesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetQRCodes_ShouldReturnQRCodes()
    {
        // Arrange

        var qrCode = new QRCodeFake(factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("qr-codes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var qrCodesResponse = await response.Content.ReadFromJsonAsync<PagedList<GetQRCodes.Response>>();
        qrCodesResponse.Should().NotBeNull();
        qrCodesResponse!.Items.Should().HaveCount(1);

        var qrCodeResponse = qrCodesResponse.Items[0];
        qrCodeResponse.DisplayName.Should().Be(qrCode.DisplayName);
        qrCodeResponse.RedirectUrl.Should().Be(qrCode.RedirectUrl);
        qrCodeResponse.ShortCode.Should().Be(qrCode.ShortCode);
        qrCodeResponse.DotStyle.Should().Be(qrCode.DotStyle);
        qrCodeResponse.CornerDotStyle.Should().Be(qrCode.CornerDotStyle);
        qrCodeResponse.CornerSquareStyle.Should().Be(qrCode.CornerSquareStyle);
        qrCodeResponse.Color.Should().Be(qrCode.Color);
    }
}
