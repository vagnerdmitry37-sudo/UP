using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using UP.Api.Db;
using UP.IntegrationTests.Helpers;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Fixtures
{
    public class ShareFixture : IAsyncLifetime
    {
        public AuthHelper Auth { get; private set; } = null!;
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
            Client = Factory.CreateClient();
            await MigrateDatabaseAsync();
            Auth = new AuthHelper(Factory);
        }

        public async Task DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
            await _container.DisposeAsync();
        }

        private async Task MigrateDatabaseAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.MigrateAsync();
        }
    }
}
