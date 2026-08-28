using UP.Api.Db;

namespace UP.Api.Services;

public interface IUnitOfWorkService
{
    Task SaveChangesAsync();
}

public class UnitOfWorkService(AppDbContext context) : IUnitOfWorkService
{
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
