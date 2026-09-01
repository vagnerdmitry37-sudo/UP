using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Services;

[ApiController]
public class AuthController(IAuthControllerService acs) : ControllerBase
{
    private readonly IAuthControllerService _acs = acs;

    [HttpPost(AuthRouts.Me)]
    public async Task<IActionResult> Me()
    {
        await _acs.MeAsync();

        return Ok();
    }

    [HttpPost(AuthRouts.Register)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _acs.RegisterAsync(request);
        return Created();
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Login)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        await _acs.LoginAsync(request);
        return Ok();
    }

    [HttpPost(AuthRouts.Logout)]
    public async Task<IActionResult> Logout()
    {
        await _acs.LogoutAsync();
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Refresh)]
    public async Task<IActionResult> Refresh()
    {
        await _acs.RefreshAsync();
        return Ok();
    }
}
