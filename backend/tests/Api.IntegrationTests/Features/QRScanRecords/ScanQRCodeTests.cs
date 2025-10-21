using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.QRScanRecords;

[Collection("IntegrationTests")]
public class ScanQRCodeTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    public ScanQRCodeTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ScanQRCode_ShouldRedirectAndCreateScanRecord()
    {
        // Arrange
        var qrCode = new QRCodeFake(_factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync($"scan/{qrCode.ShortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be(qrCode.RedirectUrl);

        var scanRecord = await AppDbContext.ScanRecords.SingleOrDefaultAsync();
        scanRecord.Should().NotBeNull();
        scanRecord!.QRCodeId.Should().Be(qrCode.Id);
    }
}
