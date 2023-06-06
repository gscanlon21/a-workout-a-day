
namespace Web.Models.Options;

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
    /// The desired scheme of the site.
    /// 
    /// sa. https
    /// </summary>
    public string Scheme { get; set; } = null!;

    /// <summary>
    /// The domain name of the site.
    /// 
    /// sa. aworkoutaday.com
    /// </summary>
    public string Domain { get; set; } = null!;

    /// <summary>
    /// Link to the site's source code.
    /// 
    /// sa. https://github.com/gscanlon21/a-workout-a-day
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Get the root domain sans TLD and sans subdomains.
    /// 
    /// sa. aworkoutaday
    /// </summary>
    public string ApexDomainSansTLD => Domain.Split('.')[^2];

    /// <summary>
    /// A url of the site.
    /// 
    /// sa. https://aworkoutaday.com
    /// </summary>
    public Uri Uri => new UriBuilder(Scheme, Domain).Uri;
}
