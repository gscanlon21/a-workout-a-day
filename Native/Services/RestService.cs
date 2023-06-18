using Native.Core;
using Native.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Native.Services;

public class RestService
{
    HttpClient _client;
    JsonSerializerOptions _serializerOptions;

    public NewsletterModel? Newsletter { get; private set; }

    public RestService()
    {
        _client = new HttpClient();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<NewsletterModel?> RefreshDataAsync()
    {
        var email = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
        var token = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null);

        if (email == null || token == null)
        {
            return null;
        }

        try
        {
            HttpResponseMessage response = await _client.GetAsync(new Uri($"https://aworkoutaday.com/n/{email}?token={token}&format=json"));
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Newsletter = JsonSerializer.Deserialize<NewsletterModel>(content, _serializerOptions);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return Newsletter;
    }
}