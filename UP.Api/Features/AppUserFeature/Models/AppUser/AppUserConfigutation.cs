using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UP.Api.Features.AppUserFeature.Models.AppUser;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUserModel>
{
    public void Configure(EntityTypeBuilder<AppUserModel> builder)
    {
        builder
            .HasOne(u => u.AuthUser)
            .WithOne()
            .HasForeignKey<AppUserModel>(u => u.AuthUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
