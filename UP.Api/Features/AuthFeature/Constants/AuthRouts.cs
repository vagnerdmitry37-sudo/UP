namespace UP.Api.Features.AuthFeature.Constants;

public static class AuthRouts
{
    public const string Base = "api/auth";

    public const string Me = $"{Base}/me";
    public const string Login = $"{Base}/login";
    public const string Logout = $"{Base}/logout";
    public const string Refresh = $"{Base}/refresh";
    public const string Register = $"{Base}/register";
}
