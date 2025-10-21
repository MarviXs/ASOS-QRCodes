using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Auth.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class RegisterTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@test.com";
        var request = new Register.Endpoint.Request(email, "StrongP@ssword123");

        // Act
        var response = await Client.PostAsJsonAsync("auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdUser = await AppDbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new Register.Endpoint.Request("admin@test.com", "password");

        // Act
        var response = await Client.PostAsJsonAsync("auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShouldReturnValidationProblem_WhenEmailInvalid()
    {
        // Arrange
        var request = new Register.Endpoint.Request("invalid-email", string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync("auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Email");
        problem.Errors.Should().ContainKey("Password");
    }
}
