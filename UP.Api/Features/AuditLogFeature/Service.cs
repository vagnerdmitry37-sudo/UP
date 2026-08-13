using Mapster;
using UP.Api.Enums;

namespace UP.Api.Features.AuditLogFeature;

public interface IAuditLogService
{
    Task AddRangeAsync(ICollection<AuditLogDto> auditLogDtos);
    AuditLog ToAuditLog(int who, EntityActions what, EntityType where, string newJson, string? oldJson);
}

public class AuditLogService(IAudiLogRepository ar) : IAuditLogService
{
    private readonly IAudiLogRepository _ar = ar;

    public Task AddRangeAsync(ICollection<AuditLogDto> auditLogDTO)
    {
        var auditLogs = auditLogDTO.Select(a => a.Adapt<AuditLog>());
        return _ar.AddRangeAsync(auditLogs);
    }

    public AuditLog ToAuditLog(int who, EntityActions what, EntityType where, string newJSON, string? oldJSON)
    {
        return new AuditLog
        {
            Who = who,
            What = what,
            When = DateTimeOffset.UtcNow,
            Where = where,
            OldJson = oldJSON,
            NewJson = newJSON
        };
    }
}
