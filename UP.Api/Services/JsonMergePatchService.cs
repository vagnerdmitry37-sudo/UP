using System.Text.Json;
using System.Text.Json.Nodes;

namespace UP.Api.Services;

public interface IJsonMergePatchService
{
    JsonDocument Apply(JsonDocument source, JsonDocument patch);
}

public sealed class JsonMergePatchService : IJsonMergePatchService
{
    public JsonDocument Apply(JsonDocument source, JsonDocument patch)
    {
        var sourceNode = JsonNode.Parse(source.RootElement.GetRawText());
        var patchNode = JsonNode.Parse(patch.RootElement.GetRawText());

        var result = ApplyMergePatch(sourceNode, patchNode);

        return JsonDocument.Parse(result?.ToJsonString() ?? "null");
    }

    private static JsonNode? ApplyMergePatch(
        JsonNode? target,
        JsonNode? patch)
    {
        // RFC 7396:
        // A non-object patch replaces the target completely.
        if (patch is not JsonObject patchObject)
        {
            return patch?.DeepClone();
        }

        var targetObject = target as JsonObject ?? [];

        foreach (var property in patchObject)
        {
            var propertyName = property.Key;
            var patchValue = property.Value;

            if (patchValue is null)
            {
                targetObject.Remove(propertyName);
                continue;
            }

            targetObject[propertyName] = ApplyMergePatch(targetObject[propertyName], patchValue);
        }

        return targetObject;
    }
}
