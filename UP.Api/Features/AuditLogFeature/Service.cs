using Mapster;
using UP.Api.Enums;

namespace UP.Api.Features.AuditLogFeature
{
    public interface IAuditLogService
    {
        Task AddRangeAsync(ICollection<AuditLogDto> auditLogDtos);
        AuditLog ToAuditLog(int who, EntityActions what, EntityType Where, string newJson, string? oldJson);
    }

    public class AuditLogService(IAudiLogRepository ar) : IAuditLogService
    {
        private readonly IAudiLogRepository _ar = ar;

        public Task AddRangeAsync(ICollection<AuditLogDto> auditLogDtos)
        {
            var auditLogs = auditLogDtos.Select(a => a.Adapt<AuditLog>());
            return _ar.AddRangeAsync(auditLogs);
        }

        public AuditLog ToAuditLog(int who, EntityActions what, EntityType Where, string newJson, string? oldJson)
        {
            return new AuditLog
            {
                Who = who,
                What = what,
                When = DateTimeOffset.UtcNow,
                Where = Where,
                OldJson = oldJson,
                NewJson = newJson
            };
        }
    }
}
