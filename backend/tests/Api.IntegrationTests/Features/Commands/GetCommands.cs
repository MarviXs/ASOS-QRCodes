using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.Commands.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class GetCommandsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetCommands_ShouldReturnCommands()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        AppDbContext.DeviceTemplates.Add(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var command = new CommandFake(deviceTemplate.Id).Generate();
        await AppDbContext.Commands.AddAsync(command);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("commands");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var commandsResponse = await response.Content.ReadFromJsonAsync<PagedList<GetCommands.Response>>();
        commandsResponse.Should().NotBeNull();
        commandsResponse!.Items.Should().HaveCount(1);

        var commandResponse = commandsResponse.Items[0];
        commandResponse.Name.Should().Be(command.Name);
        commandResponse.Params.Should().Equal(command.Params);
    }
}
