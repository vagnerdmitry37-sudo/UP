namespace UP.Api.Features.AuthFeature.Options;

public sealed class JwtOptions
{
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}
