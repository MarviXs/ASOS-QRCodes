using System.Data.Common;
using System.Security.Claims;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Fei.Is.Api.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Guid DefaultUserId { get; set; } =  Guid.Parse("00000000-0000-0000-0000-000000000001");
    private Respawner? _respawner;

    private readonly PostgreSqlContainer _appDbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine3.20")
        .WithDatabase("is-sdg")
        .WithUsername("is")
        .WithPassword("password")
        .Build();
    private DbConnection _appDbConnection = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            //             var connectionString = configuration.GetConnectionString("PostgresConnection");
            // services.AddDbContextPool<AppDbContext>(options => options.UseNpgsql(connectionString));
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseNpgsql(_appDbContainer.GetConnectionString());
            });

            services.Configure<TestAuthHandlerOptions>(options =>
            {
                options.DefaultUserId = DefaultUserId;
            });

            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                    o.DefaultChallengeScheme = TestAuthHandler.TestScheme;
                })
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _appDbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await SeedDB(scope);
        await InitializeRespawn(dbContext);
    }

    private async Task InitializeRespawn(AppDbContext dbContext)
    {
        _appDbConnection = new NpgsqlConnection(_appDbContainer.GetConnectionString());
        await _appDbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _appDbConnection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["public"] }
        );
    }

    private async Task SeedDB(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var testUser = new ApplicationUser
        {
            Id = DefaultUserId,
            UserName = "admin@test.com",
            Email = "admin@test.com",
        };
        await userManager.CreateAsync(testUser, "password");
        await userManager.AddClaimAsync(testUser, new Claim(ClaimTypes.Role, "Admin"));

        var testUser2 = new ApplicationUser
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            UserName = "admin2@test.com",
            Email = "admin2@test.com",
        };
        await userManager.CreateAsync(testUser2, "password");
        await userManager.AddClaimAsync(testUser2, new Claim(ClaimTypes.Role, "Admin"));

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
    {
        await _appDbConnection.DisposeAsync();
        await _appDbContainer.DisposeAsync();
    }

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_appDbConnection);
        await SeedDB(Services.CreateScope());
    }
}
