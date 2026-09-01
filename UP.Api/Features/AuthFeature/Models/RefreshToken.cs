namespace UP.Api.Features.AuthFeature.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool IsActive => DateTimeOffset.UtcNow < ExpiresAt && RevokedAt == null;

    public int AuthUserId { get; set; }
    public AuthUser AuthUser { get; set; } = null!;
}
