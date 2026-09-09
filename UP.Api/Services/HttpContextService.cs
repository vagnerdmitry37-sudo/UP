using System.Security.Claims;

namespace UP.Api.Services;

public interface IHttpContextService
{
    string? FindRequestCookieByKey(string key);
    string? GetCurrentAuthUserId();
    void AppendResponseCookie(string name, string value, CookieOptions options);
    void DeleteResponseCookie(string key, CookieOptions option);
}

public class HttpContextService(IHttpContextAccessor accessor) : IHttpContextService
{
    private readonly IHttpContextAccessor _accessor = accessor;
    private IResponseCookies? ResponseCookies => _accessor.HttpContext?.Response.Cookies;
    public string? FindRequestCookieByKey(string key) => _accessor.HttpContext?.Request.Cookies[key];
    public void AppendResponseCookie(string name, string value, CookieOptions options) => ResponseCookies?.Append(name, value, options);
    public string? GetCurrentAuthUserId() => _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public void DeleteResponseCookie(string key, CookieOptions option) => ResponseCookies?.Delete(key, option);
}
