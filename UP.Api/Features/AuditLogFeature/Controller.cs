using Microsoft.AspNetCore.Mvc;
using UP.Api.Db;

[ApiController]
[Route("api/audit-log")]
public class AuditLogController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult> Create()
    {
        return Created();
    }
}
