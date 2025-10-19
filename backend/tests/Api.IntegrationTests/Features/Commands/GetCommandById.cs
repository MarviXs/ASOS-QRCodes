using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Commands.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class GetCommandByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetCommandById_ShouldReturnCommand()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();
        var command = new CommandFake(deviceTemplate.Id).Generate();
        
        await AppDbContext.Commands.AddAsync(command);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(command).State = EntityState.Detached;

        // Act
        var response = await Client.GetAsync($"commands/{command.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var commandResponse = await response.Content.ReadFromJsonAsync<GetCommandById.Response>();
        commandResponse.Should().NotBeNull();
        commandResponse!.Name.Should().Be(command.Name);
        commandResponse.Params.Should().Equal(command.Params);
    }
}
