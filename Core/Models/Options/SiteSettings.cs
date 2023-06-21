
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
    public string Name { get; set; } = null!;

    /// <summary>
    /// Link to the site's source code.
    /// 
    /// sa. https://github.com/gscanlon21/a-workout-a-day
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// The link to the main website.
    /// 
    /// sa. https://aworkoutaday.com
    /// </summary>
    public string WebLink { get; set; } = null!;
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
    public string ApexDomainSansTLD => Domain.Split('.')[^2];

    /// <summary>
    /// The link to the cdn.
    /// 
    /// sa. https://cdn.aworkoutaday.com
    /// </summary>
    public string CdnLink { get; set; } = null!;
    public Uri CdnUri => new(CdnLink);

    /// <summary>
    /// The path to the api.
    /// 
    /// sa. https://aworkoutaday.com/api
    /// </summary>
    public string ApiLink { get; set; } = null!;
    public Uri ApiUri => new(ApiLink);
}
