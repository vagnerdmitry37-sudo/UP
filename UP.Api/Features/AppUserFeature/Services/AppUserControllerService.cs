using System.Text.Json;
using Mapster;
using UP.Api.Features.AppUserFeature.Responses;
using UP.Api.Services;

namespace UP.Api.Features.AppUserFeature.Services;

public interface IAppUserControllerService
{
    Task UpdateViewAsync(JsonDocument view);
    Task<AppUserResponse> MeAsync();
}

public class AppUserControllerService(
    IAppUserService aus,
    IDbContextService dcs) : IAppUserControllerService
{
    private readonly IAppUserService _aus = aus;
    private readonly IDbContextService _dcs = dcs;

    public async Task<AppUserResponse> MeAsync()
    {
        var appUser = await _aus.FindAppUserByAuthUserId();
        return appUser.Adapt<AppUserResponse>();
    }

    public async Task UpdateViewAsync(JsonDocument view)
    {
        await _dcs.SaveChangesAsync();
    }
}
