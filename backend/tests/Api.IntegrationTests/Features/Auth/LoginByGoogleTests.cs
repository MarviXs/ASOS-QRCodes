using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Auth.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class LoginByGoogleTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task LoginByGoogle_ShouldReturnValidationProblem_WhenTokenMissing()
    {
        // Arrange
        var request = new LoginByGoogle.Request(string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync("auth/google", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("GoogleToken");
    }

    [Fact]
    public async Task LoginByGoogle_ShouldReturnUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var request = new LoginByGoogle.Request("invalid-token");

        // Act
        var response = await Client.PostAsJsonAsync("auth/google", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
