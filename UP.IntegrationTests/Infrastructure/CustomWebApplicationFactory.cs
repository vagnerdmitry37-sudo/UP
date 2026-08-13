using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UP.Api.Db;

namespace UP.IntegrationTests.Infrastructure
{
    public sealed class CustomWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        private readonly string _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((options, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "4A8E7D3C91F2B6A5E8C1D9F4A7B2C6E9F1A3D5B7C8E2F4A6G9H1J3K5L7M9N2P"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
                });
            });
        }
    }
}