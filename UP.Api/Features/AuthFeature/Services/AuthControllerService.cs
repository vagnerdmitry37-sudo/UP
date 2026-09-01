using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface IAuthControllerService
{
    Task MeAsync();
    Task<AuthUser> RegisterAsync(RegisterRequest request);
    Task LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task RefreshAsync();
}

public class AuthControllerService(
    ITokenService ts,
    IAuthRepository ar,
    IDbContextService dbcs,
    IHttpContextService hcs) : IAuthControllerService
{
    private readonly ITokenService _ts = ts;
    private readonly IAuthRepository _ar = ar;
    private readonly IDbContextService _dbcs = dbcs;
    private readonly IHttpContextService _hcs = hcs;

    public async Task MeAsync() => await _ar.FindAuthUserByIdAsync(_hcs.GetCurrentAuthUserId());

    public async Task<AuthUser> RegisterAsync(RegisterRequest request)
    {
        var existingAuthUser = await _ar.FindAuthUserByEmailAsync(request.Email);
        if (existingAuthUser != null)
        {
            throw new AuthError("An account with this email already exists.");
        }

        var newAuthUser = new AuthUser()
        {
            Email = request.Email,
            UserName = request.Email,
        };

        var identityResult = await _ar.CreateAuthUserAsync(newAuthUser, request.Password);
        if (!identityResult.Succeeded)
        {
            string errorMessage = string.Join(", ", identityResult.Errors.Select(x => x.Description));
            throw new AuthError(errorMessage);
        }

        return newAuthUser;
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var authUser = await ValidateAuthUserAsync(request)
            ?? throw new AuthError("Invalid user");
        await _ts.MarkCurrentRefreshTokenAsRevokedAsync();
        var newRefreshToken = _ts.RestoreTokens(authUser);
        authUser.RefreshTokens.Add(newRefreshToken);
        await _ar.UpdateAuthUserAsync(authUser);
    }

    public async Task RefreshAsync()
    {
        var validRefreshToken = await _ts.ValidateRefreshToken()
            ?? throw new AuthError("Invalid refresh token");
        validRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
        var authUser = validRefreshToken.AuthUser;
        _ts.MarkExcessRefreshTokensAsRevoked(validRefreshToken.AuthUser);
        var newRefreshToken = _ts.RestoreTokens(authUser);
        authUser.RefreshTokens.Add(newRefreshToken);
        await _ar.UpdateAuthUserAsync(authUser);
    }

    public async Task LogoutAsync()
    {
        _ts.DeleteTokensFromResponseCookie();
        await _ts.MarkCurrentRefreshTokenAsRevokedAsync();
        await _dbcs.SaveChangesAsync();
    }

    private async Task<AuthUser?> ValidateAuthUserAsync(LoginRequest request)
    {
        var authUser = await _ar.FindAuthUserByEmailAsync(request.Email);
        if (authUser is null)
        {
            return null;
        }

        var isPasswordValid = await _ar.CheckPasswordAsync(authUser, request.Password);
        if (!isPasswordValid)
        {
            return null;
        }

        return authUser;
    }
}
