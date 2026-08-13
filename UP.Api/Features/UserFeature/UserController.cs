using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.UserFeature;

[ApiController]
[Route(UserRoutes.Base)]
public class UserController(IUserService us) : ControllerBase
{
    private readonly IUserService _us = us;

    [HttpPost]
    public async Task<ActionResult> Create()
    {
        return Created();
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok("Hello");
    }
}
