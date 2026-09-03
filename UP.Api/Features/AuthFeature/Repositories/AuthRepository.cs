using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models;

namespace UP.Api.Features.AuthFeature.Repositories;

public interface IAuthRepository
{
    Task<IdentityResult> CreateAuthUserAsync(AuthUser authUser, string password);
    Task<IdentityResult> UpdateAuthUserAsync(AuthUser authUser);
    Task<AuthUser?> FindAuthUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(AuthUser authUser, string password);
    Task<AuthUser?> FindAuthUserByIdAsync(string id);
    Task<RefreshToken?> FindCurrentRefreshTokenAsync(string refreshTokenValue);
    void RemoveRefreshTokens(ICollection<RefreshToken> refreshTokens);
}

public class AuthRepository(
    AppDbContext context,
    UserManager<AuthUser> manager) : IAuthRepository
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<AuthUser> _manager = manager;

    public async Task<IdentityResult> CreateAuthUserAsync(AuthUser authUser, string password)
        => await _manager.CreateAsync(authUser, password);
    public async Task<IdentityResult> UpdateAuthUserAsync(AuthUser authUser) => await _manager.UpdateAsync(authUser);
    public async Task<AuthUser?> FindAuthUserByEmailAsync(string email) => await _manager.FindByEmailAsync(email);
    public async Task<AuthUser?> FindAuthUserByIdAsync(string id) => await _manager.FindByIdAsync(id);
    public async Task<bool> CheckPasswordAsync(AuthUser authUser, string password)
        => await _manager.CheckPasswordAsync(authUser, password);
    public async Task<RefreshToken?> FindCurrentRefreshTokenAsync(string tokenHash) =>
         await _context.RefreshTokens
            .Include(r => r.AuthUser)
            .ThenInclude(a => a.RefreshTokens)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    public void RemoveRefreshTokens(ICollection<RefreshToken> refreshTokens) => _context.RefreshTokens.RemoveRange(refreshTokens);
}
