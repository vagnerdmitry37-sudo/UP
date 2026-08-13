namespace UP.Api.Features.AuthFeature.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool IsActive => RevokedAt == null && ExpiresAt > DateTimeOffset.UtcNow;
    public int AuthUserId { get; set; }
    public AuthUser AuthUser { get; set; } = null!;
}
