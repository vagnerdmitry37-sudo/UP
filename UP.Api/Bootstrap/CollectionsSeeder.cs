using Microsoft.EntityFrameworkCore;
using UP.Api.Features.CollectionFeature.Models.Collection;
using UP.Api.Features.CollectionsFeature.Constants;

namespace UP.Api.Bootstrap;

public static class CollectionsSeeder
{
    public static void Seed(ModelBuilder builder)
    {
        var collections = ConfigurationsLabels.Keys.Select((label, index) => new CollectionModel
        {
            Id = index + 1,
            Label = label,
            SortedBy = "Label"
        });

        builder.Entity<CollectionModel>().HasData(collections);
    }
}
