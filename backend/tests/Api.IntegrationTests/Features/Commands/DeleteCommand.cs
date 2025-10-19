using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Commands;

[Collection("IntegrationTests")]
public class DeleteCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteCommand_ShouldReturnNoContent()
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
        var response = await Client.DeleteAsync($"commands/{command.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedCommand = await AppDbContext.Commands.FindAsync(command.Id);
        deletedCommand.Should().BeNull();
    }
}
