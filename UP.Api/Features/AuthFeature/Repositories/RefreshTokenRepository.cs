using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models;

namespace UP.Api.Features.AuthFeature.Repositories;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> FindRefreshTokenByValue(string value);
}

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken) =>
        await _context.RefreshTokens.AddAsync(refreshToken);

    public async Task<RefreshToken?> FindRefreshTokenByValue(string value) =>
        await _context.RefreshTokens.Include(t => t.AuthUser).FirstOrDefaultAsync(t => t.Value == value);
}
