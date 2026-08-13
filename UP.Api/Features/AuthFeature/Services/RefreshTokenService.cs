using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models;

namespace UP.Api.Features.AuthFeature.Services;

public interface IRefreshTokenService
{
    RefreshToken GenerateToken();
    Task RevokeOldTokensAsync(int authUserId);
}

public class RefreshTokenService(AppDbContext context) : IRefreshTokenService
{
    private readonly AppDbContext _context = context;

    private readonly int _maxActiveTokens = 4;

    public async Task RevokeOldTokensAsync(int authUserId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.AuthUserId == authUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        foreach (var token in tokens.Skip(_maxActiveTokens))
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }
    }

    public RefreshToken GenerateToken()
    {
        return new RefreshToken
        {
            Value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }
}
