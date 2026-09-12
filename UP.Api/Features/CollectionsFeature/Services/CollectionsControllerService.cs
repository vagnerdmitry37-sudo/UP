using UP.Api.Features.CollectionFeature.Models.Collection;
using UP.Api.Features.CollectionFeature.Repositories;

namespace UP.Api.Features.CollectionFeature.Services;

public interface ICollectionControllerService
{
    Task<ICollection<CollectionModel>> FindAllCollectionsAsync();
}

public class CollectionsControllerService(
    ICollectionRepository cr) : ICollectionControllerService
{
    private readonly ICollectionRepository _cr = cr;

    public async Task<ICollection<CollectionModel>> FindAllCollectionsAsync()
    {
        return await _cr.FindAllCollectionsAsync();
    }
}
