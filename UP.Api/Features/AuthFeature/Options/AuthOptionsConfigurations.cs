namespace UP.Api.Features.AuthFeature.Options;

public static class AuthOptionsConfigurations
{
    private static void JwtOptions(WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<JwtOptions>()
            .BindConfiguration("Options:Jwt")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Key),
                "JWT signing key must be configured.")
            .Validate(options =>
                options.Key.Length >= 32,
                "JWT signing key must be at least 32 characters long.")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer),
                "JWT issuer must be configured.")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Audience),
                "JWT audience must be configured.")
            .ValidateOnStart();
    }

    private static void RootUserOptions(WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<RootUserOptions>()
            .BindConfiguration("Options:RootUser")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Email),
                "RootUser email must be configured.")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Password),
                "RootUser password must be configured.")
            .ValidateOnStart();
    }

    private static void TokensOptions(WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<TokensOptions>()
            .BindConfiguration("Options:Tokens")
            .Validate(options =>
                options.MaxConcurrentFamilies > 0,
                "Maximum concurrent families must be greater than 0.")
            .Validate(options =>
                options.AccessTokenLifetimeMinutes > 0,
                "Access token lifetime must be greater than 0 minutes.")
            .Validate(options =>
                options.RefreshTokenLifetimeMinutes > 0,
                "Refresh token lifetime must be greater than 0 minutes.")
            .ValidateOnStart();
    }

    public static void Init(WebApplicationBuilder builder)
    {
        JwtOptions(builder);
        RootUserOptions(builder);
        TokensOptions(builder);
    }
}
