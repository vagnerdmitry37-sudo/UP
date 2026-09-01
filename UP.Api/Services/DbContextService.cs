using UP.Api.Db;

namespace UP.Api.Services;

public interface IDbContextService
{
    Task SaveChangesAsync();
}

public class DbContextService(AppDbContext context) : IDbContextService
{
    public async Task SaveChangesAsync()
    {
        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
