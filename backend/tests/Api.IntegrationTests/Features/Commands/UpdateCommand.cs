using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Commands.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class UpdateCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateCommand_ShouldReturnNoContent()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var command = new CommandFake(deviceTemplate.Id).Generate();
        await AppDbContext.Commands.AddAsync(command);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(command).State = EntityState.Detached;
        var updateCommandRequest = new UpdateCommandRequestFake().Generate();

        // Act
        var response = await Client.PutAsJsonAsync($"commands/{command.Id}", updateCommandRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedCommand = await AppDbContext.Commands.FindAsync(command.Id);
        updatedCommand.Should().NotBeNull();
        updatedCommand!.Name.Should().Be(updateCommandRequest.Name);
        updatedCommand.Params.Should().Equal(updateCommandRequest.Params);
    }
}

public class UpdateCommandRequestFake : Faker<UpdateCommand.Request>
{
    public UpdateCommandRequestFake()
    {
        CustomInstantiator(
            f => new UpdateCommand.Request(f.Commerce.ProductName(), f.Commerce.ProductName(), [f.Random.Double(), f.Random.Double()])
        );
    }
}
