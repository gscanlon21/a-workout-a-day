using System.Text.Json;

namespace Core.Code;

public static class Mapper
{
    public static T? AsType<T>(this object from) where T : new()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(from));
    }
}
