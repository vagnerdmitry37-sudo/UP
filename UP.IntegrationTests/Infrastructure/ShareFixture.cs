using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Repositories;

namespace UP.IntegrationTests.Infrastructure;

public class ShareFixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;
    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("integration-tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Factory = new CustomWebApplicationFactory(_container.GetConnectionString());
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        await MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task<LoginRequest> RegisterRootAuthUser()
    {
        var scope = Factory.Services.CreateScope();

        var authUser = new AuthUserModel
        {
            Email = "testRoot@mail.com",
            UserName = "TestRoot",
        };

        var loginRequest = new LoginRequest
        {
            Email = authUser.Email,
            Password = "Password123@"
        };

        await scope.ServiceProvider.GetRequiredService<IAuthRepository>()
            .CreateAuthUserAsync(authUser, loginRequest.Password);

        return loginRequest;
    }

    private async Task MigrateDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
