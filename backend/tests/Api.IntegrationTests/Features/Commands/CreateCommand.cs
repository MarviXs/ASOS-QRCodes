using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Commands.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class CreateCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateCommand_ShouldReturnCreated()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var commandRequest = new CreateCommandRequestFake(deviceTemplate.Id).Generate();

        // Act
        var response = await Client.PostAsJsonAsync("commands", commandRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        var createdCommand = await AppDbContext.Commands.FindAsync(createdId);

        createdCommand.Should().NotBeNull();
        createdCommand!.Name.Should().Be(commandRequest.Name);
        createdCommand!.DisplayName.Should().Be(commandRequest.DisplayName);
        createdCommand.Params.Should().Equal(commandRequest.Params);
    }
}

public class CreateCommandRequestFake : Faker<CreateCommand.Request>
{
    public CreateCommandRequestFake(Guid deviceTemplateId)
    {
        CustomInstantiator(
            f =>
                new CreateCommand.Request(
                    f.Commerce.ProductName(),
                    f.Commerce.ProductName(),
                    deviceTemplateId,
                    [f.Random.Double(), f.Random.Double()]
                )
        );
    }
}
