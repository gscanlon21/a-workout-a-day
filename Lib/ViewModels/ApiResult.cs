using System.Net;
using System.Net.Http.Json;

namespace Lib.ViewModels;

public enum StatusCodeRange
{
    Ok = 2,
    ClientError = 4,
    ServerError = 5,
}

public class ApiResult<T>
{
    private ApiResult(HttpStatusCode status, T? result)
    {
        Result = result;
        StatusCode = (int)status switch
        {
            >= 200 and < 300 => StatusCodeRange.Ok,
            >= 400 and < 500 => StatusCodeRange.ClientError,
            >= 500 and < 600 => StatusCodeRange.ServerError,
            _ => throw new NotImplementedException(),
        };
    }

    public StatusCodeRange StatusCode { get; set; }
    public T? Result { get; set; }

    public static async Task<ApiResult<T>> FromResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            return new ApiResult<T>(response.StatusCode, await response.Content.ReadFromJsonAsync<T>());
        }

        return new ApiResult<T>(response.StatusCode, default);
    }
}
