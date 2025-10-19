using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Features.Sensors.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Sensors;

[Collection("IntegrationTests")]
public class CreateSensorTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateSensor_ShouldReturnCreated()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var sensorRequest = new CreateSensorRequestFake().Generate();

        // Act
        var response = await Client.PostAsJsonAsync($"device-templates/{deviceTemplate.Id}/sensors", sensorRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseString = await response.Content.ReadAsStringAsync();

        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        var createdSensor = await AppDbContext.Sensors.FindAsync(createdId);
        createdSensor.Should().NotBeNull();
        createdSensor!.Name.Should().Be(sensorRequest.Name);
        createdSensor.Tag.Should().Be(sensorRequest.Tag);
        createdSensor.Unit.Should().Be(sensorRequest.Unit);
        createdSensor.AccuracyDecimals.Should().Be(sensorRequest.AccuracyDecimals);
    }
}

public class CreateSensorRequestFake : Faker<CreateSensor.Request>
{
    public CreateSensorRequestFake()
    {
        CustomInstantiator(
            f => new CreateSensor.Request(f.Commerce.ProductAdjective(), f.Commerce.ProductName(), f.Random.AlphaNumeric(2), f.Random.Int(0, 4))
        );
    }
}
