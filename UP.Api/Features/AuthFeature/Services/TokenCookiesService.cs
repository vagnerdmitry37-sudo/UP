using Microsoft.Extensions.Options;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface ITokenCookiesService
{
    void SetTokenCookies(string accessToken, string refreshTokenValue);
    void DeleteTokensCookies();
}

public class TokenCookiesService(
    IHttpContextService hcs,
    IOptions<AuthOptions> authOptions,
    IWebHostEnvironment environment
    ) : ITokenCookiesService
{
    private readonly IHttpContextService _hcs = hcs;
    private readonly AuthOptions _authOptions = authOptions.Value;
    private readonly IWebHostEnvironment _environment = environment;

    public void SetTokenCookies(string accessToken, string refreshTokenValue)
    {
        var (accessTokenOptions, refreshTokenOptions) = CreateTokensCookieOptions();

        _hcs.AppendResponseCookie(TokenNames.AccessToken, accessToken, accessTokenOptions);
        _hcs.AppendResponseCookie(TokenNames.RefreshToken, refreshTokenValue, refreshTokenOptions);
    }

    public void DeleteTokensCookies()
    {
        var (accessTokenOptions, refreshTokenOptions) = CreateTokensCookieOptions();

        _hcs.DeleteResponseCookie(TokenNames.AccessToken, accessTokenOptions);
        _hcs.DeleteResponseCookie(TokenNames.RefreshToken, refreshTokenOptions);
    }

    private (CookieOptions, CookieOptions) CreateTokensCookieOptions()
    {
        var accessTokenOptions = CreateCookieOptions("/", _authOptions.AccessTokenLifetimeMinutes);
        var refreshTokenOptions = CreateCookieOptions(AuthRouts.Base, _authOptions.RefreshTokenLifetimeMinutes);

        return (accessTokenOptions, refreshTokenOptions);
    }

    private CookieOptions CreateCookieOptions(string path, int expiresInSeconds) => new()
    {
        HttpOnly = true,
        Secure = !_environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = path,
        Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInSeconds)
    };
}
