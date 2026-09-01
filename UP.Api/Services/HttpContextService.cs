using System.Security.Claims;
using UP.Api.Features.AppErrorFeature;

namespace UP.Api.Services;

public interface IHttpContextService
{
    string? FindRequestCookie(string key);
    string GetCurrentAuthUserId();
    void AppendResponseCookie(string name, string value, CookieOptions options);
    void DeleteResponseCookie(string key, CookieOptions? option);
}

public class HttpContextService(IHttpContextAccessor accessor) : IHttpContextService
{
    private readonly IHttpContextAccessor _accessor = accessor;
    private IResponseCookies ResponseCookies => _accessor.HttpContext?.Response.Cookies
        ?? throw new AuthError("Response cookies not found");

    public string? FindRequestCookie(string key) => _accessor.HttpContext?.Request.Cookies[key];

    public void AppendResponseCookie(string name, string value, CookieOptions options) => ResponseCookies.Append(name, value, options);

    public string GetCurrentAuthUserId()
    {
        return _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User could not be identified");
    }

    public void DeleteResponseCookie(string key, CookieOptions? option = null) => ResponseCookies.Delete(key, option);
}
