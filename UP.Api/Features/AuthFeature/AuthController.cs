using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AuthFeature;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService aus, ITokenService ts) : ControllerBase
{
    private readonly IAuthService _aus = aus;
    private readonly ITokenService _ts = ts;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _aus.Login(request);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _aus.Register(request);
        return Created();
    }

    [AllowAnonymous]
    [HttpGet("validate-access-token")]
    public async Task<IActionResult> ValidateAccessToken()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = _ts.ValidateAccessToken(token);
        return Ok(result);
    }
}