using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.AppUserFeature.Constants;

[ApiController]
[Route(UserRoutes.Base)]
public class AppUserController() : ControllerBase
{
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
