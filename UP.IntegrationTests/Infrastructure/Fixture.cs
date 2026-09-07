using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using UP.Api.Db;

namespace UP.IntegrationTests.Infrastructure;

public class Fixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;
    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("integration-tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    private Respawner _respawner = null!;
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();
        Factory = new CustomWebApplicationFactory(_connectionString);
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await MigrateDatabaseAsync();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    private async Task MigrateDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
