using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AppUserFeature.Models;
using UP.Api.Features.AppUserFeature.Repositories;
using UP.Api.Services;

namespace UP.Api.Features.AppUserFeature.Services;

public interface IAppUserService
{
    Task<AppUserModel> FindAppUserByAuthUserId();
}

public class AppUserService(
    IAppUserRepository aur,
    IHttpContextService hcs) : IAppUserService
{
    private readonly IAppUserRepository _aur = aur;
    private readonly IHttpContextService _hcs = hcs;

    public async Task<AppUserModel> FindAppUserByAuthUserId()
    {
        var currentAuthUserIdString = _hcs.GetCurrentAuthUserId() ?? throw new AuthError("Authenticated user ID is missing");

        if (int.TryParse(currentAuthUserIdString, out int currentAuthUserIdInt))
        {
            return await _aur.FindAppUserByAuthUserId(currentAuthUserIdInt) ?? throw new AuthError("Authenticated user ID is invalid");
        }
        else
        {
            throw new AuthError("Authenticated user was not found");
        }
    }
}
