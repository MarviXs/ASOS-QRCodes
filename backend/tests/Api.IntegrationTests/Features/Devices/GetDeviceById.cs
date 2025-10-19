using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Devices.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class GetDeviceByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceById_ShouldReturnDevice()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"devices/{device.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var deviceResponse = await response.Content.ReadFromJsonAsync<GetDeviceById.Response>();
        deviceResponse.Should().NotBeNull();
        deviceResponse!.Name.Should().Be(device.Name);
        deviceResponse.AccessToken.Should().Be(device.AccessToken);
        deviceResponse.DeviceTemplate?.Id.Should().Be(deviceTemplate.Id);
    }
}
