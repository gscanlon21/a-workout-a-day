
namespace Web.Models.Options;

public class SiteSettings
{
    /// <summary>
    /// The name of the website
    /// </summary>
    public string Name { get; set; } = null!;

    public string Scheme { get; set; } = null!;
    public string Host { get; set; } = null!;
    
    public Uri Uri => new UriBuilder(Scheme, Host).Uri;
}
