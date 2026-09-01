namespace UP.Api.Features.AuthFeature.Constants;

public static class AuthConstants
{
    public const int MaxConcurrentDevices = 5;
    public const int AccessTokenLifetimeSeconds = 60 * 15; // 15 minutes
    public const int RefreshTokenLifetimeSeconds = 60 * 60 * 3; // 3 hours 
}
