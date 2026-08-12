using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UP.Api.Db;

namespace UP.IntegrationTests.Infrastructure
{
    public sealed class CustomWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        private readonly string _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
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