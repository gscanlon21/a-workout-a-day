using System.ComponentModel.DataAnnotations;

namespace Core.Models.Options;

/// <summary>
/// App settings for the domain name.
/// </summary>
public class SiteSettings
{
    /// <summary>
    /// The user-friendly name of the website.
    /// 
    /// sa. A Workout A Day
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Link to the site's source code.
    /// 
    /// sa. https://github.com/gscanlon21/a-workout-a-day
    /// </summary>
    [Required]
    public string Source { get; init; } = null!;

    /// <summary>
    /// The link to the main website.
    /// 
    /// sa. https://aworkoutaday.com
    /// </summary>
    [Required]
    public string WebLink { get; init; } = null!;
    public Uri WebUri => new(WebLink);

    /// <summary>
    /// The domain name of the site.
    /// 
    /// sa. aworkoutaday.com
    /// </summary>
    public string Domain => WebUri.Host;

    /// <summary>
    /// Get the root domain sans TLD and sans subdomains.
    /// 
    /// sa. aworkoutaday
    /// </summary>
    public string ApexDomainSansTLD => Domain.Contains('.') ? Domain.Split('.')[^2] : Domain;

    /// <summary>
    /// The link to the cdn.
    /// 
    /// sa. https://cdn.aworkoutaday.com
    /// </summary>
    [Required]
    public string CdnLink { get; init; } = null!;
    public Uri CdnUri => new(CdnLink);

    /// <summary>
    /// The path to the api.
    /// 
    /// sa. https://aworkoutaday.com/api
    /// </summary>
    [Required]
    public string ApiLink { get; init; } = null!;
    public Uri ApiUri => new(ApiLink);
}
