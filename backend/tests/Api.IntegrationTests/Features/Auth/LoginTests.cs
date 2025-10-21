using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Auth.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class LoginTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    public LoginTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturnTokensForValidCredentials()
    {
        // Arrange
        var request = new Login.Request("admin@test.com", "password");

        // Act
        var response = await Client.PostAsJsonAsync("auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<Login.Response>();
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResponse.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refreshToken = await AppDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == _factory.DefaultUserId);
        refreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        // Arrange
        var request = new Login.Request("admin@test.com", "wrong-password");

        // Act
        var response = await Client.PostAsJsonAsync("auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnValidationProblem_WhenEmailInvalid()
    {
        // Arrange
        var request = new Login.Request("invalid-email", string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync("auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Email");
        problem.Errors.Should().ContainKey("Password");
    }
}
