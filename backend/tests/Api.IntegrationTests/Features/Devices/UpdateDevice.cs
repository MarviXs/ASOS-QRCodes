using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Devices.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class UpdateDeviceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateDevice_ShouldReturnNoContent()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(device).State = EntityState.Detached;
        var updateDeviceRequest = new UpdateDeviceRequestFake(deviceTemplate.Id).Generate();

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"devices/{device.Id}", updateDeviceRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedDevice = await AppDbContext.Devices.FindAsync(device.Id);

        updatedDevice.Should().NotBeNull();
        updatedDevice!.Name.Should().Be(updateDeviceRequest.Name);
        updatedDevice.AccessToken.Should().Be(updateDeviceRequest.AccessToken);
        updatedDevice.DeviceTemplateId.Should().Be(device.DeviceTemplateId);
    }
}

public class UpdateDeviceRequestFake : Faker<UpdateDevice.Request>
{
    public UpdateDeviceRequestFake(Guid templateId)
    {
        CustomInstantiator(f => new UpdateDevice.Request(f.Commerce.ProductName(), f.Internet.Mac(), templateId, DeviceConnectionProtocol.HTTP));
    }
}
