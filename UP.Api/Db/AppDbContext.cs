using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UP.Api.Enums;
using UP.Api.Features.AuditLogFeature;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Models.RefreshToken;
using UP.Api.Features.CollectionFeature.Models;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Models;

namespace UP.Api.Db;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AuthUserModel, IdentityRole<int>, int>(options)
{
    public DbSet<AppUserModel> AppUsers => Set<AppUserModel>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Excursion> Excursions => Set<Excursion>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        SetData(builder);
    }

    private static void SetData(ModelBuilder builder)
    {
        ICollection<EntityType> collectionNames = [EntityType.Transfers, EntityType.Excursions];
        var collections = collectionNames.Select((n, i) => new Collection
        {
            Id = i + 1,
            Name = n.ToString(),
            SortedBy = "Name"
        });

        builder.Entity<Collection>().HasData(collections);
    }
}
