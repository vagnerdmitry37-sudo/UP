using UP.Api.Enums;

namespace UP.Api.Features.AuditLogFeature;

public class AuditLogDto
{
    public int? Id { get; set; }
    public int? Who { get; set; }
    public EntityActions What { get; set; }
    public DateTimeOffset When { get; set; }
    public EntityType EffectedEntityType { get; set; }
    public int EffectedEntityId { get; set; }
    public string? OldJson { get; set; }
    public string? NewJson { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public int? Who { get; set; }
    public EntityActions What { get; set; }
    public DateTimeOffset When { get; set; }
    public EntityType Where { get; set; }
    public string? OldJson { get; set; }
    public string? NewJson { get; set; }
}
