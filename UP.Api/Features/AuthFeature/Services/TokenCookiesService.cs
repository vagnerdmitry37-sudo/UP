using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Services;

namespace UP.Api.Features.AuthFeature.Services;

public interface ITokenCookiesService
{
    void SetTokenCookies(string assesToken, string refreshTokenValue);
    void DeleteTokensCookies();
}

public class TokenCookiesService(
    IHttpContextService hcs,
    IWebHostEnvironment environment
    ) : ITokenCookiesService
{
    private readonly IHttpContextService _hcs = hcs;
    private readonly IWebHostEnvironment _environment = environment;

    public void SetTokenCookies(string assesToken, string refreshTokenValue)
    {
        var (accessTokenOptions, refreshTokenOptions) = CreateTokensCookieOptions();

        _hcs.AppendResponseCookie(TokenNames.AccessToken, assesToken, accessTokenOptions);
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
        var accessTokenOptions = CreateCookieOptions("/", AuthConstants.AccessTokenLifetimeSeconds);
        var refreshTokenOptions = CreateCookieOptions(AuthRouts.Base, AuthConstants.RefreshTokenLifetimeSeconds);

        return (accessTokenOptions, refreshTokenOptions);
    }

    private CookieOptions CreateCookieOptions(string path, int expiresInSeconds) => new()
    {
        HttpOnly = true,
        Secure = !_environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = path,
        Expires = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds)
    };
}
