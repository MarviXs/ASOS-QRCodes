using System.Security.Claims;
using Fei.Is.Api.Data.Contexts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fei.Is.Api.IntegrationTests;

public class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly AppDbContext AppDbContext;

    protected readonly HttpClient Client;

    private readonly Func<Task> _resetDatabase;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        AppDbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _resetDatabase = factory.ResetDatabase;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _resetDatabase();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
