using App.Dtos.Footnote;
using App.ViewModels.User;
using Core.Models.Footnote;
using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace App.Services;

public class FootnoteService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SiteSettings> _siteSettings;

    public FootnoteService(HttpClient httpClient, IOptions<SiteSettings> siteSettings)
    {
        _siteSettings = siteSettings;
        _httpClient = httpClient;
        if (_httpClient.BaseAddress != _siteSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.ApiUri;
        }
    }

    public async Task<IList<Footnote>> GetFootnotes(UserNewsletterViewModel user, int count = 1, FootnoteType ofType = FootnoteType.All)
    {
        return await _httpClient.GetFromJsonAsync<List<Footnote>>($"{_siteSettings.Value.ApiUri.AbsolutePath}/footnote/GetFootnotes");
    }
}
