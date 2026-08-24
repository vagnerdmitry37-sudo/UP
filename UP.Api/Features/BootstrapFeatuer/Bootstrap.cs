using Microsoft.EntityFrameworkCore;
using UP.Api.Db;

namespace UP.Api.Features.BootstrapFeatuer;

public static class Bootstrap
{
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        await RootUserSeeder.SeedAsync(scope.ServiceProvider);
    }
}
