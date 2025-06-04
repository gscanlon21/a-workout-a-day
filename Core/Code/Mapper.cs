using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Code;

public static class Mapper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        // Reduce the size of the serilized string for memory usage.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static T? AsType<T>(this object from) where T : new()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(from, Options), Options);
    }
}
