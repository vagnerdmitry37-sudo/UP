using System.Text.Json;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Services;

namespace UP.Api.Features.AppUserFeature.Services;

public interface IAppUserControllerService
{
    Task UpdateViewAsync(JsonDocument view);
    Task<AppUserDto> MeAsync();
}

public class AppUserControllerService(
    IAppUserService aus,
    IDbContextService dcs) : IAppUserControllerService
{
    private readonly IAppUserService _aus = aus;
    private readonly IDbContextService _dcs = dcs;

    public async Task<AppUserDto> MeAsync()
    {
        var appUser = await _aus.FindAppUserByAuthUserId();
        return appUser.Adapt<AppUserDto>();
    }

    public async Task UpdateViewAsync([FromBody] JsonDocument view)
    {
        var appUser = await _aus.FindAppUserByAuthUserId();
        appUser.View = view;
        await _dcs.SaveChangesAsync();
    }
}
