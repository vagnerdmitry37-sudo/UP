namespace UP.Api.Features.AuthFeature.Options;

public sealed class TokensOptions
{
    public required int MaxConcurrentFamilies { get; init; }
    public required int AccessTokenLifetimeMinutes { get; init; }
    public required int RefreshTokenLifetimeMinutes { get; init; }
}
