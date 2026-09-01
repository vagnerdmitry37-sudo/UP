using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface ITokenService
{
    RefreshToken RestoreTokens(AuthUser authUser);
    string GenerateAccessToken(AuthUser authUser);
    (string refreshTokenValue, RefreshToken refreshToken) GenerateRefreshToken(int authUserId);
    Task<RefreshToken?> ValidateRefreshToken();
    void DeleteTokensFromResponseCookie();
    void MarkExcessRefreshTokensAsRevoked(AuthUser authUser);
    Task MarkCurrentRefreshTokenAsRevokedAsync();
}

public class TokenService(
    IAuthRepository ar,
    IHttpContextService hcs,
    IConfiguration config) : ITokenService
{
    private readonly IAuthRepository _ar = ar;
    private readonly IHttpContextService _hcs = hcs;
    private readonly IConfiguration _config = config;

    private readonly CookieOptions _accessTokenCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddSeconds(AuthConstants.AccessTokenLifetimeSeconds),
    };

    private readonly CookieOptions _refreshTokenCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/api/auth",
        Expires = DateTimeOffset.UtcNow.AddSeconds(AuthConstants.RefreshTokenLifetimeSeconds),
    };

    public string GenerateAccessToken(AuthUser authUser)
    {
        ArgumentNullException.ThrowIfNull(authUser.Email);

        Claim[] claims = [
                new (JwtRegisteredClaimNames.Sub, authUser.Id.ToString(CultureInfo.InvariantCulture)),
                new (JwtRegisteredClaimNames.Email, authUser.Email)
            ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AuthConstants.AccessTokenLifetimeSeconds),
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
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(AuthConstants.RefreshTokenLifetimeSeconds),
                AuthUserId = authUserId,
            }
        );
    }

    public async Task<RefreshToken?> ValidateRefreshToken()
    {
        var refreshTokenValue = _hcs.FindRequestCookie(TokenNames.RefreshToken);
        if (refreshTokenValue is null)
        {
            return null;
        }
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));
        var refreshToken = await _ar.FindRefreshTokenByValueAsync(tokenHash);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        return refreshToken;
    }

    public void MarkExcessRefreshTokensAsRevoked(AuthUser authUser)
    {
        var tokensToRemove = authUser.RefreshTokens
            .Where(r => r.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(AuthConstants.MaxConcurrentDevices - 1)
            .ToList();

        if (tokensToRemove.Count > 0)
        {
            _ar.RemoveRefreshTokens(tokensToRemove);
        }
    }

    public async Task MarkCurrentRefreshTokenAsRevokedAsync()
    {
        var refreshTokenFromCookies = _hcs.FindRequestCookie(TokenNames.RefreshToken);
        if (refreshTokenFromCookies is not null)
        {
            var refreshToken = await _ar.FindRefreshTokenByValueAsync(refreshTokenFromCookies);
            if (refreshToken is not null && refreshToken.IsActive)
            {
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    public RefreshToken RestoreTokens(AuthUser authUser)
    {
        var (refreshTokenValue, newRefreshtoken) = GenerateRefreshToken(authUser.Id);
        _hcs.AppendResponseCookie(TokenNames.AccessToken, GenerateAccessToken(authUser), _accessTokenCookieOptions);
        _hcs.AppendResponseCookie(TokenNames.RefreshToken, refreshTokenValue, _refreshTokenCookieOptions);
        return newRefreshtoken;
    }

    public void DeleteTokensFromResponseCookie()
    {
        _hcs.DeleteResponseCookie(TokenNames.AccessToken, _accessTokenCookieOptions);
        _hcs.DeleteResponseCookie(TokenNames.RefreshToken, _refreshTokenCookieOptions);
    }
}
