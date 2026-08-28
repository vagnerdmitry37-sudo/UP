using System.Security.Claims;
using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface IAuthService
{
    Task MeAsync();
    Task<AuthUser> RegisterAsync(RegisterRequest request);
    Task LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task RefreshAsync();
}

public class AuthService(
    IUnitOfWorkService uows,
    IAccessTokenService ats,
    IAuthUserRepository aur,
    IRefreshTokenService rts,
    IRefreshTokenRepository rtr,
    IHttpContextAccessor httpca) : IAuthService
{
    private readonly IUnitOfWorkService _uows = uows;
    private readonly IAccessTokenService _ats = ats;
    private readonly IAuthUserRepository _aur = aur;
    private readonly IRefreshTokenService _rts = rts;
    private readonly IRefreshTokenRepository _rtr = rtr;
    private readonly IHttpContextAccessor _httpca = httpca;

    private string CurrentUserId => _httpca.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new AuthError("CurrentUserId not found");

    private string RefreshTokenFromCookie => _httpca.HttpContext?.Request.Cookies[TokenNames.RefreshToken]
        ?? throw new AuthError("RefreshToken not found");

    private IResponseCookies ResponseCookies => _httpca.HttpContext?.Response.Cookies
        ?? throw new AuthError("Response cookies not found");

    public async Task MeAsync()
    {
        var user = await _aur.FindByIdAsync(CurrentUserId);
    }

    public async Task<AuthUser> RegisterAsync(RegisterRequest request)
    {
        var existingAuthUser = await _aur.FindByEmailAsync(request.Email);
        if (existingAuthUser != null)
        {
            throw new AuthError("An account with this email already exists.");
        }

        var newAuthUser = new AuthUser()
        {
            Email = request.Email,
            UserName = request.Email,
        };

        var identityResult = await _aur.CreateAsync(newAuthUser, request.Password);
        if (!identityResult.Succeeded)
        {
            string errorMessage = string.Join(", ", identityResult.Errors.Select(x => x.Description));
            throw new AuthError(errorMessage);
        }

        return newAuthUser;
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var validAuthUser = await ValidateAuthUserAsync(request);
        await _rts.RevokeOldTokensAsync(validAuthUser.Id);
        var newRefreshToken = _rts.GenerateToken(validAuthUser.Id);
        await _rtr.AddRefreshTokenAsync(newRefreshToken);
        await _uows.SaveChangesAsync();
        await _aur.UpdateAsync(validAuthUser);

        SetCookie(
            accessToken: _ats.GenerateToken(validAuthUser),
            refreshToken: newRefreshToken.Value
        );
    }

    public async Task LogoutAsync()
    {
        var refreshToken = RefreshTokenFromCookie;

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var token = await _rtr.FindRefreshTokenByValue(refreshToken);

            if (token is not null && token.IsActive)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                await _uows.SaveChangesAsync();
            }
        }

        ResponseCookies.Delete(TokenNames.AccessToken);
        ResponseCookies.Delete(TokenNames.RefreshToken);
    }

    public async Task RefreshAsync()
    {
        var refreshToken = await _rtr.FindRefreshTokenByValue(RefreshTokenFromCookie);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            throw new AuthError("Refresh token validation failed.");
        }

        var authUser = refreshToken.AuthUser;

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        var newRefreshToken = _rts.GenerateToken(authUser.Id);

        await _rtr.AddRefreshTokenAsync(newRefreshToken);

        await _uows.SaveChangesAsync();

        SetCookie(
            accessToken: _ats.GenerateToken(refreshToken.AuthUser),
            refreshToken: newRefreshToken.Value
        );
    }

    private async Task<AuthUser> ValidateAuthUserAsync(LoginRequest request)
    {
        string errorMessage = "Not valid email or password";

        var authUser = await _aur.FindByEmailAsync(request.Email)
            ?? throw new AuthError(errorMessage);

        var isPasswordValid = await _aur.CheckPasswordAsync(authUser, request.Password);
        if (!isPasswordValid)
        {
            throw new AuthError(errorMessage);
        }

        return authUser;
    }

    private void SetCookie(string accessToken, string refreshToken)
    {
        IEnumerable<(string Name, string Value, string Path)> tokens =
        [
            (TokenNames.AccessToken, accessToken, "/"),
            (TokenNames.RefreshToken, refreshToken, "/api/auth")
        ];

        foreach (var (name, value, path) in tokens)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                Path = path
            };

            if (_httpca.HttpContext is null)
            {
                throw new AuthError("HttpContext not found");
            }

            ResponseCookies.Append(name, value, options);
        }
    }
}
