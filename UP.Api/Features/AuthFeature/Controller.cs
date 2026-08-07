using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AuthFeature;

[ApiController]
[Route(AuthRouts.Base)]
public class AuthController(IAuthService aus, ITokenService ts) : ControllerBase
{
    private readonly IAuthService _aus = aus;
    private readonly ITokenService _ts = ts;

    [AllowAnonymous]
    [HttpPost(AuthRouts.Login)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _aus.Login(request);
        if (result == null) return Unauthorized();

        return Ok(result);
    }

    [HttpPost(AuthRouts.Register)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _aus.Register(request);
        return Created();
    }

    [AllowAnonymous]
    [HttpGet(AuthRouts.ValidateAccessToken)]
    public async Task<IActionResult> ValidateAccessToken()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = _ts.ValidateAccessToken(token);

        return Ok(result);
    }
}