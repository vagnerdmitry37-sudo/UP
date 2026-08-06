using UP.Api.Enums;

namespace UP.Api.Features.CollectionFeature
{
    public class CollectionDto
    {
        public int? Id { get; set; }
        public EntityType EntityType { get; set; }
        public string SortedBy { get; set; } = "";
    }

    public class Collection
    {
        public int Id { get; set; }
        public EntityType EntityType { get; set; }
        public string SortedBy { get; set; } = "";
    }
}
