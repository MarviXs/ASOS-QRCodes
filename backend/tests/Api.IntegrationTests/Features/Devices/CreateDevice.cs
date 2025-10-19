using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Devices.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class CreateDeviceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateDevice_ShouldReturnCreated()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();
    
        var deviceRequest = new CreateDeviceRequestFake(deviceTemplate.Id).Generate();

        // Act
        var response = await Client.PostAsJsonAsync("devices", deviceRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        var createdDevice = await AppDbContext.Devices.FindAsync(createdId);
        createdDevice.Should().NotBeNull();
        createdDevice!.Name.Should().Be(deviceRequest.Name);
        createdDevice.AccessToken.Should().Be(deviceRequest.AccessToken);
        createdDevice.DeviceTemplateId.Should().Be(deviceRequest.TemplateId);
    }
}

public class CreateDeviceRequestFake : Faker<CreateDevice.Request>
{
    public CreateDeviceRequestFake(Guid templateId)
    {
        CustomInstantiator(f => new CreateDevice.Request(f.Commerce.ProductName(), f.Internet.Mac(), templateId, DeviceConnectionProtocol.HTTP));
    }
}
