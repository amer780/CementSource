using System.Text.Json.Nodes;

namespace CementGB.Mod.Utilities;

public static class RESTUtilities
{
    public static bool TryGetPropertyValueAs<T>(this JsonObject thisObject, string propertyKey, out T? castedValue)
    {
        castedValue = default;

        if (thisObject.TryGetPropertyValue(propertyKey, out var jsonNode) && jsonNode is not null)
            if (jsonNode.AsValue().TryGetValue(out castedValue)) return true;

        return false;
    }
}
