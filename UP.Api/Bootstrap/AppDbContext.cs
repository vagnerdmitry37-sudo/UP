using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;
using UP.Api.Features.CollectionFeature.Models.Collection;
using UP.Api.Models;

namespace UP.Api.Bootstrap;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AuthUserModel, IdentityRole<int>, int>(options)
{
    public DbSet<AppUserModel> AppUsers => Set<AppUserModel>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Excursion> Excursions => Set<Excursion>();
    public DbSet<CollectionModel> Collections => Set<CollectionModel>();
    public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        CollectionsSeeder.Seed(builder);
    }
}
