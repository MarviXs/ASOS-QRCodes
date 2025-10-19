using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Devices.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class GetDevicesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDevices_ShouldReturnDevices()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("devices");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var devicesResponse = await response.Content.ReadFromJsonAsync<PagedList<GetDevices.Response>>();
        devicesResponse.Should().NotBeNull();
        devicesResponse!.Items.Should().NotBeEmpty();

        var deviceResponse = devicesResponse.Items.First();
        deviceResponse.Name.Should().Be(device.Name);

        devicesResponse.TotalCount.Should().Be(1);
        devicesResponse.CurrentPage.Should().Be(1);
    }
}
