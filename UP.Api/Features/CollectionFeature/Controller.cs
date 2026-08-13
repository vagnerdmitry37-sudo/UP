using Microsoft.AspNetCore.Mvc;
using UP.Api.Db;
using UP.Api.Features.CollectionFeature;

[ApiController]
[Route("api/collection")]
public class Controller(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpPost]
    public async Task<ActionResult> Create(ICollection<CollectionDto> collectionDtos)
    {
        return Created();
    }
}
