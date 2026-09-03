using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
