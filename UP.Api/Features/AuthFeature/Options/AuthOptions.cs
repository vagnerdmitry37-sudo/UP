namespace UP.Api.Features.AuthFeature.Options;

public class AuthOptions
{
    public required int MaxConcurrentDevices { get; init; }
    public required int AccessTokenLifetimeMinutes { get; init; }
    public required int RefreshTokenLifetimeMinutes { get; init; }
}
