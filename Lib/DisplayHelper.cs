﻿using Core.Dtos.User;
using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Lib;

public class DisplayHelper(IOptions<SiteSettings> siteSettings)
{
    private readonly IOptions<SiteSettings> _siteSettings = siteSettings;

    public string NewsletterLink(UserNewsletterDto? user, int id)
    {
        if (user == null)
        {
            return string.Empty;
        }

        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/n/{Uri.EscapeDataString(user.Email)}/{id}?token={Uri.EscapeDataString(user.Token)}";
    }

    public string UserLink(UserNewsletterDto? user, string toPath)
    {
        if (user == null)
        {
            return string.Empty;
        }

        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/u/{Uri.EscapeDataString(user.Email)}/{toPath.Trim('/')}?token={Uri.EscapeDataString(user.Token)}";
    }

    public string UserActiveLink(UserNewsletterDto? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        //toPath = $"u/{Uri.EscapeDataString(_User.Email)}/{toPath.Trim('/')}?token={Uri.EscapeDataString(_User.Token)}";
        return $"{_siteSettings.Value.WebLink.TrimEnd('/')}/u/{Uri.EscapeDataString(user.Email)}/r?token={Uri.EscapeDataString(user.Token)}"; // &to={Uri.EscapeDataString(toPath)}
    }

    /*
    public string ExternalActiveLink(string toUrl)
    {
        if (_User == null)
        {
            return toUrl;
        }

        return $"{_siteSettings.Value.WebLink}/u/{Uri.EscapeDataString(_User.Email)}/r?to={Uri.EscapeDataString(toUrl)}&token={Uri.EscapeDataString(_User.Token)}";
    }
    */
}