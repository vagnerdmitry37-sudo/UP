using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using UP.Api.Bootstrap;
using UP.Api.Features.AppUserFeature.Models.AppUser;
using UP.Api.Features.AuthFeature.Options;

namespace UP.Api.Features.AuthFeature.Models.AuthUser;

public class AuthUserConfiguration : IEntityTypeConfiguration<AuthUserModel>
{
    public void Configure(EntityTypeBuilder<AuthUserModel> builder)
    {
        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(r => r.AuthUser)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public static class RootUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var options = services.GetRequiredService<IOptions<RootUserOptions>>().Value;
        var userManager = services.GetRequiredService<UserManager<AuthUserModel>>();

        var rootUser = await userManager.FindByEmailAsync(options.Email);

        if (rootUser is null)
        {
            rootUser = new AuthUserModel
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(rootUser, options.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        var appUserExists = await context.AppUsers.AnyAsync(x => x.AuthUserId == rootUser.Id);

        if (!appUserExists)
        {
            var appUser = new AppUserModel
            {
                Name = "Root",
                Email = rootUser.Email!,
                AuthUserId = rootUser.Id
            };

            context.AppUsers.Add(appUser);

            await context.SaveChangesAsync();
        }
    }
}
