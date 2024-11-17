using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace Lib.Services;

public enum StatusCodeRange
{
    Informational = 1,
    Successful = 2,
    Redirection = 3,
    ClientError = 4,
    ServerError = 5,
}

[DebuggerDisplay("{StatusCode}: {GetValueOrDefault()}")]
public class ApiResult<T>
{
    private readonly T? _value;

    private ApiResult(HttpStatusCode status, T? value)
    {
        _value = value;
        StatusCode = (int)status switch
        {
            >= 100 and < 200 => StatusCodeRange.Informational,
            >= 200 and < 300 => StatusCodeRange.Successful,
            >= 300 and < 400 => StatusCodeRange.Redirection,
            >= 400 and < 500 => StatusCodeRange.ClientError,
            >= 500 and < 600 => StatusCodeRange.ServerError,
            _ => throw new NotImplementedException(),
        };
    }

    public T Value => _value!;

    public bool HasValue => _value != null;

    public T? GetValueOrDefault() => _value;
    public T GetValueOrDefault(T val) => _value ?? val;

    public StatusCodeRange StatusCode { get; private set; }

    /// <summary>
    /// Http status code in the 200s.
    /// </summary>
    public bool IsSuccessStatusCode => StatusCode == StatusCodeRange.Successful;

    public static async Task<ApiResult<T>> FromResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            return new ApiResult<T>(response.StatusCode, await response.Content.ReadFromJsonAsync<T>());
        }

        return new ApiResult<T>(response.StatusCode, default);
    }
}
