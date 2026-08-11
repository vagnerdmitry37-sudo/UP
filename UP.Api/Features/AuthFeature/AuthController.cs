using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AuthFeature;
using UP.Api.Features.AuthFeature.Requests;
using UP.Api.Features.AuthFeature.Responses;
using UP.Api.Features.AuthFeature.Services;

[ApiController]
[Route(AuthRouts.Base)]
public class AuthController(IAuthService aus) : ControllerBase
{
    private readonly IAuthService _aus = aus;


    [HttpPost(AuthRouts.Register)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _aus.Register(request);
        return Created();
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Login)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _aus.Login(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost(AuthRouts.Refresh)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _aus.Refresh(request);
        return Ok(result);
    }
}