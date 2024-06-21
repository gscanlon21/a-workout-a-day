using System.Text.Json;

namespace Web.Code;

public static class Mapper
{
    public static T? AsType<T, F>(this F from) where T : new()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(from));
    }
}
