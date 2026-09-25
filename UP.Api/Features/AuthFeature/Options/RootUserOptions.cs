namespace UP.Api.Features.AuthFeature.Options;

public sealed class RootUserOptions
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
