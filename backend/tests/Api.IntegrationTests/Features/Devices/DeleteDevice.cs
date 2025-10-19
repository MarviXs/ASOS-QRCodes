using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class DeleteDeviceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteDevice_ShouldReturnNoContent()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(device).State = EntityState.Detached;

        // Act
        var response = await Client.DeleteAsync($"devices/{device.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedDevice = await AppDbContext.Devices.FindAsync(device.Id);
        deletedDevice.Should().BeNull();
    }
}
