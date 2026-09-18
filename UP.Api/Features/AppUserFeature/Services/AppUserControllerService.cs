using Mapster;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Features.AppUserFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AppUserFeature.Services;

public interface IAppUserControllerService
{
    Task<AppUserDto> MeAsync();
}

public class AppUserControllerService(
    IHttpContextService hcs,
    IAppUserRepository aur) : IAppUserControllerService
{
    private readonly IHttpContextService _hcs = hcs;
    private readonly IAppUserRepository _aur = aur;

    public async Task<AppUserDto> MeAsync()
    {
        var currentAuthUserIdString = _hcs.GetCurrentAuthUserId() ?? throw new AuthError("No user id");

        if (int.TryParse(currentAuthUserIdString, out int currentAuthUserIdInt))
        {
            var appUser = await _aur.FindAppUserByAuthUserId(currentAuthUserIdInt) ?? throw new AuthError("User not found");
            return appUser.Adapt<AppUserDto>();
        }
        else
        {
            throw new AuthError("No user id");
        }
    }
}
