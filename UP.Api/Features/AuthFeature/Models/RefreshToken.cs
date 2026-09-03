namespace UP.Api.Features.AuthFeature.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = "";

    public string DeviceId { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; } = null;

    public bool IsActive => DateTimeOffset.UtcNow < ExpiresAt && RevokedAt == null;

    public Guid FamilyId { get; set; }

    public int? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }

    public int AuthUserId { get; set; }
    public AuthUser AuthUser { get; set; } = null!;
}
