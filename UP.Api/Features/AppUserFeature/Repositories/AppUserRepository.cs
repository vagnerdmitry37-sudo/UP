using UP.Api.Db;
using UP.Api.Features.AppUserFeature.Models;

namespace UP.Api.Features.AppUserFeature.Repositories;

public interface IAppUserRepository
{
    Task Add(AppUserModel user);
}

public class AppUserRepository(AppDbContext context) : IAppUserRepository
{
    public async Task Add(AppUserModel appUser) => await context.AppUsers.AddAsync(appUser);
}
