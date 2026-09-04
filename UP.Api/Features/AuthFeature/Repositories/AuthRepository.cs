using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;

namespace UP.Api.Features.AuthFeature.Repositories;

public interface IAuthRepository
{
    Task<IdentityResult> CreateAuthUserAsync(AuthUserModel authUser, string password);
    Task<IdentityResult> UpdateAuthUserAsync(AuthUserModel authUser);
    Task<AuthUserModel?> FindAuthUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(AuthUserModel authUser, string password);
    Task<AuthUserModel?> FindAuthUserByIdAsync(string id);
    Task<RefreshTokenModel?> FindCurrentRefreshTokenAsync(string refreshTokenValue);
    void RemoveRefreshTokens(ICollection<RefreshTokenModel> refreshTokens);
}

public class AuthRepository(
    AppDbContext context,
    UserManager<AuthUserModel> manager) : IAuthRepository
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<AuthUserModel> _manager = manager;

    public async Task<IdentityResult> CreateAuthUserAsync(AuthUserModel authUser, string password)
        => await _manager.CreateAsync(authUser, password);
    public async Task<IdentityResult> UpdateAuthUserAsync(AuthUserModel authUser) => await _manager.UpdateAsync(authUser);
    public async Task<AuthUserModel?> FindAuthUserByEmailAsync(string email) =>
        await _manager.Users
            .Include(u => u.RefreshTokens
                .OrderByDescending(rt => rt.CreatedAt))
        .FirstOrDefaultAsync(u => u.Email == email);
    public async Task<AuthUserModel?> FindAuthUserByIdAsync(string id) => await _manager.FindByIdAsync(id);
    public async Task<bool> CheckPasswordAsync(AuthUserModel authUser, string password)
        => await _manager.CheckPasswordAsync(authUser, password);
    public async Task<RefreshTokenModel?> FindCurrentRefreshTokenAsync(string tokenHash) =>
         await _context.RefreshTokens
            .Include(r => r.AuthUser)
            .ThenInclude(a => a.RefreshTokens)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    public void RemoveRefreshTokens(ICollection<RefreshTokenModel> refreshTokens) => _context.RefreshTokens.RemoveRange(refreshTokens);
}
