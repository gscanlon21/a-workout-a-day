using App.Dtos.Footnote;
using App.ViewModels.User;
using Core.Models.Footnote;
using System.Net.Http.Json;

namespace App.Services;

public class FootnoteService
{
    private readonly HttpClient _httpClient;

    public FootnoteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        //_httpClient.BaseAddress = new Uri("https://aworkoutaday.com");
        _httpClient.BaseAddress = new Uri("https://localhost:7107");
    }

    public async Task<IList<Footnote>> GetFootnotes(UserNewsletterViewModel user, int count = 1, FootnoteType ofType = FootnoteType.All)
    {
        return await _httpClient.GetFromJsonAsync<List<Footnote>>($"/footnote/GetFootnotes");
    }
}
