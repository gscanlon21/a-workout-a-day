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
    public string Name { get; init; } = "A Workout a Day";

    /// <summary>
    /// Link to the site's source code.
    /// 
    /// sa. https://github.com/gscanlon21/a-workout-a-day
    /// </summary>
    [Required]
    public string Source { get; init; } = "https://github.com/gscanlon21/a-workout-a-day";

    /// <summary>
    /// The link to the main website.
    /// 
    /// sa. https://aworkoutaday.com
    /// </summary>
    [Required]
    public string WebLink { get; init; } = "https://aworkoutaday.com";
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
    public string CdnLink { get; init; } = "https://cdn.aworkoutaday.com";
    public Uri CdnUri => new(CdnLink);

    /// <summary>
    /// The path to the api.
    /// 
    /// sa. https://aworkoutaday.com/api
    /// </summary>
    [Required]
    public string ApiLink { get; init; } = "https://aworkoutaday.com/api";
    public Uri ApiUri => new(ApiLink);

    /// <summary>
    /// The link where the user can download the app.
    /// </summary>
    public string AppLink { get; set; } = null!;
    public Uri AppUri => new(AppLink);
}
