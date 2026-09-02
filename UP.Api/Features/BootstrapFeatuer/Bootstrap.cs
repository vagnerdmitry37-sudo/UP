using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UP.Api.Db;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuditLogFeature;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Features.AuthFeature.Services;
using UP.Api.Features.AuthFeature.Settings;
using UP.Api.Features.UserFeature;
using UP.Api.Services;

namespace UP.Api.Features.BootstrapFeatuer;

public enum CorsMode
{
    DevelopmentCors
}

public class Bootstrap(WebApplicationBuilder builder)
{
    private void AddOptions()
    {
        builder.Services
            .AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
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

        builder.Services
            .AddOptions<AuthOptions>()
            .BindConfiguration("Auth")
            .Validate(options =>
                options.MaxConcurrentDevices > 0,
                "Maximum concurrent devices must be greater than 0.")
            .Validate(options =>
                options.AccessTokenLifetimeMinutes > 0,
                "Access token lifetime must be greater than 0 minutes.")
            .Validate(options =>
                options.RefreshTokenLifetimeMinutes > 0,
                "Refresh token lifetime must be greater than 0 minutes.")
            .ValidateOnStart();
    }

    private void AddScoped()
    {
        builder.Services.AddScoped<IDbContextService, DbContextService>();
        builder.Services.AddScoped<IHttpContextService, HttpContextService>();

        // User feature
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // AuditLog feature
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IAudiLogRepository, AudiLogRepository>();

        // Auth feature
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ITokenCookiesService, TokenCookiesService>();
        builder.Services.AddScoped<IAuthControllerService, AuthControllerService>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    }

    private string AddCors()
    {
        builder.Services.AddCors(options => options.AddPolicy(CorsMode.DevelopmentCors.ToString(), policy => policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return CorsMode.DevelopmentCors.ToString();
    }

    private void AddDbContext() =>
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    private void AddIdentityCore()
    {
        builder.Services.AddIdentityCore<AuthUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;

            options.User.RequireUniqueEmail = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
        })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    private void AddJwtBearer(JwtOptions jwtOptions)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer((options) =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies[TokenNames.AccessToken];

                    return Task.CompletedTask;
                },

                OnChallenge = contex => throw new AuthError("Access token is invalid or expired.")
            };
        });
    }

    public string Init()
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new Exception("JwtOptions should exist");

        AddOptions();
        AddScoped();
        AddDbContext();
        AddIdentityCore();
        AddJwtBearer(jwtOptions);

        var corseMode = AddCors();

        return corseMode;
    }

    public async Task RunMigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        await RootUserSeeder.SeedAsync(scope.ServiceProvider);
    }
}
