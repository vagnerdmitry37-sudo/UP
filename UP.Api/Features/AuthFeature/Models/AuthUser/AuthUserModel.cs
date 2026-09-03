using Microsoft.AspNetCore.Identity;
using UP.Api.Features.AuthFeature.Models.RefreshToken;

namespace UP.Api.Features.AuthFeature.Models.AuthUser;

public class AuthUserModel : IdentityUser<int>
{
    public ICollection<RefreshTokenModel> RefreshTokens { get; set; } = [];
}
