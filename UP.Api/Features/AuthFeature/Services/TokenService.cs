using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface ITokenService
{
    string GenerateAccessToken(AuthUserModel authUser);
    (string refreshTokenValue, RefreshTokenModel refreshToken) GenerateRefreshToken(int authUserId, Guid? familyId = null);
    void MarkExcessRefreshTokensAsRevoked(AuthUserModel authUser);
    Task<RefreshTokenModel?> FindCurrentRefreshTokenAsync();
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

    public string GenerateAccessToken(AuthUserModel authUser)
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

    public (string refreshTokenValue, RefreshTokenModel refreshToken) GenerateRefreshToken(int authUserId, Guid? familyId = null)
    {
        var now = DateTimeOffset.UtcNow;
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));

        return (
            refreshTokenValue,
            new RefreshTokenModel
            {
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(_authOptions.RefreshTokenLifetimeMinutes),
                AuthUserId = authUserId,
                FamilyId = familyId ?? Guid.NewGuid(),
            }
        );
    }

    public void MarkExcessRefreshTokensAsRevoked(AuthUserModel authUser)
    {
        var tokensToRevoke = authUser.RefreshTokens
            .Where(r => r.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(_authOptions.MaxConcurrentFamilies)
            .ToList();

        if (tokensToRevoke.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var token in tokensToRevoke)
            {
                token.RevokedAt = now;
            }
        }
    }

    public async Task<RefreshTokenModel?> FindCurrentRefreshTokenAsync()
    {
        var refreshTokenValue = _hcs.FindRequestCookie(TokenNames.RefreshToken);
        if (refreshTokenValue is null)
        {
            return null;
        }
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));
        return await _ar.FindCurrentRefreshTokenAsync(tokenHash);
    }
}
