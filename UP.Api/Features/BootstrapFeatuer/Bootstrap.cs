using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuditLogFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Features.AuthFeature.Services;
using UP.Api.Features.UserFeature;
using UP.Api.Services;
using UP.Api.Features.AuthFeature.Constants;

namespace UP.Api.Features.BootstrapFeatuer;

public enum CorsMode
{
    DevelopmentCors
}

public class Bootstrap(WebApplicationBuilder builder)
{
    public void AddScoped()
    {
        builder.Services.AddScoped<IUnitOfWorkService, UnitOfWorkService>();

        // User feature
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // AuditLog feature
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IAudiLogRepository, AudiLogRepository>();

        // Auth feature
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    public string AddCors()
    {
        builder.Services.AddCors(options => options.AddPolicy(CorsMode.DevelopmentCors.ToString(), policy => policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return CorsMode.DevelopmentCors.ToString();
    }

    public void AddDbContext() =>
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    public void AddIdentityCore()
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

    public void AddJwtBearer()
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = AccessTokenService.GetTokenValidationParameters(builder.Configuration);

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

    public async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        await RootUserSeeder.SeedAsync(scope.ServiceProvider);
    }
}
