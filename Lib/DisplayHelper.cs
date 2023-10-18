using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Lib;

public class DisplayHelper(NewsletterState appState, IOptions<SiteSettings> siteSettings)
{
    private readonly NewsletterState _appState = appState;
    private readonly IOptions<SiteSettings> _siteSettings = siteSettings;

    public string NewsletterLink(DateOnly today)
    {
        if (_appState.User == null)
        {
            return string.Empty;
        }

        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/n/{Uri.EscapeDataString(_appState.User.Email)}/{today:O}?token={Uri.EscapeDataString(_appState.User.Token)}";
    }

    public string UserLink(string toPath)
    {
        if (_appState.User == null)
        {
            return string.Empty;
        }

        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/u/{Uri.EscapeDataString(_appState.User.Email)}/{toPath.Trim('/')}?token={Uri.EscapeDataString(_appState.User.Token)}";
    }

    public string UserActiveLink()
    {
        if (_appState.User == null)
        {
            return string.Empty;
        }

        //toPath = $"u/{Uri.EscapeDataString(_appState.User.Email)}/{toPath.Trim('/')}?token={Uri.EscapeDataString(_appState.User.Token)}";
        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/u/{Uri.EscapeDataString(_appState.User.Email)}/r?token={Uri.EscapeDataString(_appState.User.Token)}"; // &to={Uri.EscapeDataString(toPath)}
    }

    /*
    public string ExternalActiveLink(string toUrl)
    {
        if (_appState.User == null)
        {
            return toUrl;
        }

        return $"{_siteSettings.Value.WebLink}/u/{Uri.EscapeDataString(_appState.User.Email)}/r?to={Uri.EscapeDataString(toUrl)}&token={Uri.EscapeDataString(_appState.User.Token)}";
    }
    */
}