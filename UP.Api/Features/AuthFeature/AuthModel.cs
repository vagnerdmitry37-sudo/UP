using Microsoft.AspNetCore.Identity;

namespace UP.Api.Features.AuthFeature
{
    public class AuthUser : IdentityUser<int>
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }

    public class RefreshToken
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsRevoked { get; set; }

        public int AuthUserId { get; set; }

        public AuthUser AuthUser { get; set; } = null!;
    }
}
