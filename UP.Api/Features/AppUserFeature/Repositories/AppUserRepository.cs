using Microsoft.EntityFrameworkCore;
using UP.Api.Bootstrap;
using UP.Api.Features.AppUserFeature.Models;

namespace UP.Api.Features.AppUserFeature.Repositories;

public interface IAppUserRepository
{
    Task<AppUserModel?> FindAppUserByAuthUserId(int authUserId);
}

public class AppUserRepository(
    AppDbContext context) : IAppUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<AppUserModel?> FindAppUserByAuthUserId(int authUserId)
    {
        return await _context.AppUsers.Where(u => u.AuthUserId == authUserId).FirstOrDefaultAsync();
    }
}
