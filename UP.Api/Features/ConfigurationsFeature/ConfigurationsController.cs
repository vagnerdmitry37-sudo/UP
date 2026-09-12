using Microsoft.AspNetCore.Mvc;
using UP.Api.Features.CollectionFeature.Constants;
using UP.Api.Features.CollectionFeature.Services;

[ApiController]
public class ConfigurationsController(
    ICollectionControllerService ccs) : ControllerBase
{
    private readonly ICollectionControllerService _ccs = ccs;

    [HttpGet(ConfigurationsRouts.FindAll)]
    public async Task<ActionResult> FindAllCollections()
    {
        var collections = await _ccs.FindAllCollectionsAsync();
        return Ok(collections);
    }
}
