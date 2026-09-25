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
    IOptions<TokensOptions> tokensOptions,
    IWebHostEnvironment environment
    ) : ITokenCookiesService
{
    private readonly IHttpContextService _hcs = hcs;
    private readonly TokensOptions _tokensOptions = tokensOptions.Value;
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
        var accessTokenOptions = CreateCookieOptions("/", _tokensOptions.AccessTokenLifetimeMinutes);
        var refreshTokenOptions = CreateCookieOptions($"/{AuthRouts.Base}", _tokensOptions.RefreshTokenLifetimeMinutes);

        return (accessTokenOptions, refreshTokenOptions);
    }

    private CookieOptions CreateCookieOptions(string path, int expiresInMinutes) => new()
    {
        HttpOnly = true,
        Secure = !_environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = path,
        Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
    };
}
