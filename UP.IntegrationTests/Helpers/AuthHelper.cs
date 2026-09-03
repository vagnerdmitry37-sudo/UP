using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UP.Api.Db;
using UP.Api.Features.AuthFeature.Models;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Helpers;

public class AuthHelper(CustomWebApplicationFactory factory)
{
    public async Task<(AuthUser, IdentityResult)> CreateAuthUserAsync(RegisterRequest request)
    {
        using var scope = factory.Services.CreateScope();
        var userMeneger = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

        var newAuthUser = new AuthUser
        {
            Email = request.Email,
            UserName = request.Email,
        };

        var identityResult = await userMeneger.CreateAsync(newAuthUser, request.Password);

        return (newAuthUser, identityResult);
    }

    public async Task<AuthUser?> FindAuthUserByEmailAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var userMeneger = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

        return await userMeneger.FindByEmailAsync(email);
    }

    public async Task<RefreshToken?> FindCurrentRefreshTokenAsync(string value)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == value);
    }
}
