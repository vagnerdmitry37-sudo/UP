using Microsoft.EntityFrameworkCore;
using UP.Api.Db;

namespace UP.Api.Services;

public class AppBackgroundService(
    ILogger<AppBackgroundService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await CleanupExpiredRefreshTokensAsync(cancellationToken);
    }

    private async Task CleanupExpiredRefreshTokensAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(3));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTimeOffset.UtcNow;
                var cutoff = now.AddDays(-7);

                await context.RefreshTokens
                    .Where(r => (r.ExpiresAt < now) || (r.RevokedAt != null && r.RevokedAt < cutoff))
                    .ExecuteDeleteAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refresh token cleanup failed.");
            }
        }
    }
}
