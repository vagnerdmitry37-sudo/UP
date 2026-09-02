using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Features.AuthFeature.Settings;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface ITokenService
{
    string GenerateAccessToken(AuthUser authUser);
    (string refreshTokenValue, RefreshToken refreshToken) GenerateRefreshToken(int authUserId);
    Task<RefreshToken?> ValidateRefreshToken();
    void MarkExcessRefreshTokensAsRemoved(AuthUser authUser);
    Task MarkCurrentRefreshTokenAsRevokedAsync();
}

public class TokenService(
    IAuthRepository ar,
    IHttpContextService hcs,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthOptions> authOptions) : ITokenService
{
    private readonly IAuthRepository _ar = ar;
    private readonly IHttpContextService _hcs = hcs;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly AuthOptions _authOptions = authOptions.Value;

    public string GenerateAccessToken(AuthUser authUser)
    {
        ArgumentNullException.ThrowIfNull(authUser.Email);

        Claim[] claims = [
                new (JwtRegisteredClaimNames.Email, authUser.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.Sub, authUser.Id.ToString(CultureInfo.InvariantCulture)),
            ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenLifetimeMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string refreshTokenValue, RefreshToken refreshToken) GenerateRefreshToken(int authUserId)
    {
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));

        return (
            refreshTokenValue,
            new RefreshToken
            {
                TokenHash = tokenHash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_authOptions.RefreshTokenLifetimeMinutes),
                AuthUserId = authUserId,
            }
        );
    }

    public async Task<RefreshToken?> ValidateRefreshToken()
    {
        var refreshToken = await FindRefreshTokenByValueAsync();

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        return refreshToken;
    }

    public void MarkExcessRefreshTokensAsRemoved(AuthUser authUser)
    {
        var tokensToRemove = authUser.RefreshTokens
            .Where(r => r.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(_authOptions.MaxConcurrentDevices)
            .ToList();

        if (tokensToRemove.Count > 0)
        {
            _ar.RemoveRefreshTokens(tokensToRemove);
        }
    }

    public async Task MarkCurrentRefreshTokenAsRevokedAsync()
    {
        var refreshToken = await FindRefreshTokenByValueAsync();
        if (refreshToken is not null && refreshToken.IsActive)
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        }
    }

    private async Task<RefreshToken?> FindRefreshTokenByValueAsync()
    {
        var refreshTokenValue = _hcs.FindRequestCookie(TokenNames.RefreshToken);
        if (refreshTokenValue is null)
        {
            return null;
        }
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));
        return await _ar.FindRefreshTokenByValueAsync(tokenHash);
    }
}
