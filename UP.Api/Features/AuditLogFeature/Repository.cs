using UP.Api.Db;

namespace UP.Api.Features.AuditLogFeature;

public interface IAudiLogRepository
{
    Task AddRangeAsync(IEnumerable<AuditLog> auditLogs);
}

public class AudiLogRepository(AppDbContext abc) : IAudiLogRepository
{
    private readonly AppDbContext _abc = abc;

    public Task AddRangeAsync(IEnumerable<AuditLog> auditLogs)
    {
        return _abc.AddRangeAsync(auditLogs);
    }
}
