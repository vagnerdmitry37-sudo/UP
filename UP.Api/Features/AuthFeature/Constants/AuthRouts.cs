namespace UP.Api.Features.AuthFeature.Constants;

public static class AuthRouts
{
    private const string _base = "api/auth";

    public const string Me = $"{_base}/me";
    public const string Login = $"{_base}/login";
    public const string Logout = $"{_base}/logout";
    public const string Refresh = $"{_base}/refresh";
    public const string Register = $"{_base}/register";
}
