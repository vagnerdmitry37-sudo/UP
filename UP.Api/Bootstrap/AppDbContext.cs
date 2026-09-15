using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;

namespace UP.Api.Bootstrap;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AuthUserModel, IdentityRole<int>, int>(options)
{
    public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
