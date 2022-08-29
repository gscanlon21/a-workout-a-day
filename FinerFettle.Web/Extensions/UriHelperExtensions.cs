using Microsoft.AspNetCore.Mvc;

namespace FinerFettle.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Used in the newsletter since relative links won't work in emails.
        /// </summary>
        /// <param name="contentPath">The relative path to the content asset. After wwwroot.</param>
        /// <returns>An absolute uri string to the content asset</returns>
        public static string AbsoluteContent(this IUrlHelper url, string contentPath)
        {
            HttpRequest request = url.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
        }
    }
}
