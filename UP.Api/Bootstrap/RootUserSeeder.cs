using Microsoft.AspNetCore.Identity;
using UP.Api.Features.AuthFeature.Models.AuthUser;

namespace UP.Api.BootstrapFeatuer;

public class RootUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<AuthUserModel>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        const string rootRole = "Root";

        var email = configuration["Bootstrap:Email"];
        var password = configuration["Bootstrap:Password"];

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Bootstrap:Email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Bootstrap:Password is not configured.");
        }

        if (!await roleManager.RoleExistsAsync(rootRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<int>
                {
                    Name = rootRole,
                    NormalizedName = rootRole.ToUpperInvariant()
                });

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        var rootUser = await userManager.FindByEmailAsync(email);

        if (rootUser is null)
        {
            rootUser = new AuthUserModel
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var userResult =
                await userManager.CreateAsync(rootUser, password);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", userResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(rootUser, rootRole))
        {
            var roleResult = await userManager.AddToRoleAsync(rootUser, rootRole);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(x => x.Description)));
            }
        }
    }
}
