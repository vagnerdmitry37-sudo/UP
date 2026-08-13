namespace UP.Api.Features.AuthFeature;

public static class AuthRouts
{
    private const string _base = "api/auth";

    public const string Login = $"{_base}/login";
    public const string Refresh = $"{_base}/refresh";
    public const string Register = $"{_base}/register";
}
