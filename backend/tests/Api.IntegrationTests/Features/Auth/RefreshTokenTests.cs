using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Auth.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class RefreshTokenTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task RefreshToken_ShouldReturnNewAccessToken()
    {
        // Arrange
        var loginRequest = new Login.Request("admin@test.com", "password");
        var loginResponse = await Client.PostAsJsonAsync("auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<Login.Response>();
        loginContent.Should().NotBeNull();

        var refreshRequest = new RefreshToken.Request(loginContent!.RefreshToken);

        // Act
        var response = await Client.PostAsJsonAsync("auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshToken.Response>();
        refreshResponse.Should().NotBeNull();
        refreshResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnForbiddenProblem_WhenTokenInvalid()
    {
        // Arrange
        var request = new RefreshToken.Request(Guid.NewGuid().ToString());

        // Act
        var response = await Client.PostAsJsonAsync("auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}
