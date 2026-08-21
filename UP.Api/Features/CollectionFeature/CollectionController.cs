using Microsoft.AspNetCore.Mvc;
using UP.Api.Db;

[ApiController]
[Route("api/collection")]
public class CollectionController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpPost]
    public async Task<ActionResult> Create()
    {
        return Created();
    }
}
