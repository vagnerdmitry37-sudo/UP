using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.UserFeature;

[ApiController]
[Route("api/user")]
public class UserController(IUserService us) : ControllerBase
{
    private readonly IUserService _us = us;

    [HttpPost]
    public async Task<ActionResult> Create(string userDtos)
    {
        return Created();
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok(DateTime.UtcNow);
    }
}