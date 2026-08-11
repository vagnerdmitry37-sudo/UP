using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UP.Api.Features.AuditLogFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.CollectionFeature;
using UP.Api.Models;

namespace UP.Api.Db
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AuthUser, IdentityRole<int>, int>(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<Excursion> Excursions => Set<Excursion>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}