using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Features.AuthFeature.Models.AuthUser;

namespace UP.Api.Bootstrap;

public class RootUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<AuthUserModel>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var bootstrapOptions = services.GetRequiredService<IOptions<BootstrapOptions>>().Value;
        var context = services.GetRequiredService<AppDbContext>();

        const string rootRole = "Root";

        var email = bootstrapOptions.RootUserEmail;
        var password = bootstrapOptions.RootUserPassword;

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

            var rootAppUser = new AppUserModel
            {
                Name = rootRole,
                Email = rootUser.Email,
                AuthUser = rootUser
            };

            await context.AppUsers.AddAsync(rootAppUser);
            var userResult = await userManager.CreateAsync(rootUser, password);

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
