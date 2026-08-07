using Microsoft.AspNetCore.Identity;

namespace UP.Api.Features.AuthFeature
{
    public class AuthUser : IdentityUser<int>
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsRevoked { get; set; }

        public int AuthUserId { get; set; }

        public AuthUser AuthUser { get; set; } = null!;
    }
}
