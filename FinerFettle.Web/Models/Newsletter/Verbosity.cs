namespace FinerFettle.Web.Models.Newsletter
{
    /// <summary>
    /// The detail shown in the newsletter.
    /// </summary>
    public enum Verbosity
    {
        Quiet = 1 << 0,
        Minimal = 1 << 1 | Quiet,
        Normal = 1 << 2 | Minimal,
        Detailed = 1 << 3 | Normal,
        Diagnostic = 1 << 4 | Detailed
    }
}
