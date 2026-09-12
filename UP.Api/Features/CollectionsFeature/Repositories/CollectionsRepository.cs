using Microsoft.EntityFrameworkCore;
using UP.Api.Bootstrap;
using UP.Api.Features.CollectionFeature.Models.Collection;

namespace UP.Api.Features.CollectionFeature.Repositories;

public interface ICollectionRepository
{
    Task<ICollection<CollectionModel>> FindAllCollectionsAsync();
}

public class CollectionsRepository(AppDbContext context) : ICollectionRepository
{
    private readonly AppDbContext _context = context;

    public async Task<ICollection<CollectionModel>> FindAllCollectionsAsync() => await _context.Collections.ToListAsync();
}
