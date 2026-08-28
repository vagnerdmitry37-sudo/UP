using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Requests;
using UP.Api.Features.AuthFeature.Services;

[ApiController]
public class AuthController(IAuthService aus) : ControllerBase
{
    private readonly IAuthService _aus = aus;

    [HttpPost(AuthRouts.Me)]
    public async Task<IActionResult> Me()
    {
        await _aus.MeAsync();

        return Ok();
    }

    [HttpPost(AuthRouts.Register)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _aus.RegisterAsync(request);
        return Created();
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Login)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        await _aus.LoginAsync(request);
        return Ok();
    }

    [HttpPost(AuthRouts.Logout)]
    public async Task<IActionResult> LogoutAsync()
    {
        await _aus.LogoutAsync();
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Refresh)]
    public async Task<IActionResult> RefreshAsync()
    {
        await _aus.RefreshAsync();
        return Ok();
    }
}
