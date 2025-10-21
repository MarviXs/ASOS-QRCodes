using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.QRCodes;

[Collection("IntegrationTests")]
public class DeleteQRCode(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteQRCode_ShouldReturnNoContent()
    {
        // Arrange
        var qrCode = new QRCodeFake(factory.DefaultUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(qrCode).State = EntityState.Detached;

        // Act
        var response = await Client.DeleteAsync($"qr-codes/{qrCode.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedQRCode = await AppDbContext.QRCodes.FindAsync(qrCode.Id);
        deletedQRCode.Should().BeNull();
    }

    [Fact]
    public async Task DeleteQRCode_ShouldReturnNotFound_WhenQRCodeMissing()
    {
        // Act
        var response = await Client.DeleteAsync($"qr-codes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQRCode_ShouldReturnForbid_WhenNotOwner()
    {
        // Arrange
        var qrCode = new QRCodeFake(factory.OtherUserId).Generate();
        await AppDbContext.QRCodes.AddAsync(qrCode);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"qr-codes/{qrCode.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
