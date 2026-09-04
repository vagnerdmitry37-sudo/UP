using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface IAuthControllerService
{
    Task MeAsync();
    Task<AuthUserModel> RegisterAsync(RegisterRequest request);
    Task LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task RefreshAsync();
}

public class AuthControllerService(
    ITokenService ts,
    IAuthRepository ar,
    IDbContextService dbcs,
    IHttpContextService hcs,
    ITokenCookiesService tcs) : IAuthControllerService
{
    private readonly ITokenService _ts = ts;
    private readonly IAuthRepository _ar = ar;
    private readonly IDbContextService _dbcs = dbcs;
    private readonly IHttpContextService _hcs = hcs;
    private readonly ITokenCookiesService _tcs = tcs;

    public async Task MeAsync() => await _ar.FindAuthUserByIdAsync(_hcs.GetCurrentAuthUserId());

    public async Task<AuthUserModel> RegisterAsync(RegisterRequest request)
    {
        var existingAuthUser = await _ar.FindAuthUserByEmailAsync(request.Email);
        if (existingAuthUser != null)
        {
            throw new AuthError("Registration not allowed");
        }

        var newAuthUser = new AuthUserModel()
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
        var currentRefreshToken = await _ts.FindCurrentRefreshTokenAsync();
        var authUser = await ValidateCurrentAuthUserAsync(request)
            ?? throw new AuthError("Invalid user");
        await RestoreTokens(authUser, currentRefreshToken);
    }

    public async Task RefreshAsync()
    {
        var currentRefreshToken = await ValidateCurrentRefreshToken();
        var authUser = currentRefreshToken.AuthUser;
        await RestoreTokens(authUser, currentRefreshToken);
    }

    public async Task LogoutAsync()
    {
        var currentRefreshToken = await _ts.FindCurrentRefreshTokenAsync();
        currentRefreshToken?.RevokedAt = DateTimeOffset.UtcNow;
        _tcs.DeleteTokensCookies();
        await _dbcs.SaveChangesAsync();
    }

    private async Task<RefreshTokenModel> ValidateCurrentRefreshToken()
    {
        var currentRefreshToken = await _ts.FindCurrentRefreshTokenAsync()
            ?? throw new AuthError("Invalid refresh token");

        if (currentRefreshToken.ReplacedByToken is not null)
        {
            RevokeRefreshTokenFamily(currentRefreshToken);
            await _dbcs.SaveChangesAsync();
            throw new AuthError("Refresh token reuse detected");
        }

        return currentRefreshToken;
    }

    private static void RevokeRefreshTokenFamily(RefreshTokenModel token)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var familyToken in token.AuthUser.RefreshTokens
            .Where(x => x.FamilyId == token.FamilyId && x.IsActive))
        {
            familyToken.RevokedAt = now;
        }
    }

    private async Task RestoreTokens(AuthUserModel authUser, RefreshTokenModel? currentRefreshToken = null)
    {
        var accessToken = _ts.GenerateAccessToken(authUser);
        var (refreshTokenValue, refreshToken) = _ts.GenerateRefreshToken(authUser.Id, currentRefreshToken?.FamilyId);

        if (currentRefreshToken is not null)
        {
            currentRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
            currentRefreshToken.ReplacedByToken = refreshToken;
        }

        _ts.MarkExcessRefreshTokensAsRevoked(authUser);
        _tcs.SetTokenCookies(accessToken, refreshTokenValue);
        authUser.RefreshTokens.Add(refreshToken);
        await _ar.UpdateAuthUserAsync(authUser);
    }

    private async Task<AuthUserModel?> ValidateCurrentAuthUserAsync(LoginRequest request)
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
