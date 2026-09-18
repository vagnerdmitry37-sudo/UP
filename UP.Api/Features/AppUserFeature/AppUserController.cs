using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AppUserFeature.Constants;
using UP.Api.Features.AppUserFeature.Services;

[ApiController]
public class AppUserController(
    IAppUserControllerService aucs) : ControllerBase
{
    private readonly IAppUserControllerService _aucs = aucs;

    [HttpGet(AppUserRoutes.Me)]
    public async Task<IActionResult> Me()
    {
        var appUser = await _aucs.MeAsync();
        return Ok(appUser);
    }
}

