using FinerFettle.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Components
{
    public class FootnoteViewComponent : ViewComponent
    {
        private readonly CoreContext _context;

        public FootnoteViewComponent(CoreContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footnote = await _context.Footnotes.OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();
            if (footnote == null)
            {
                return Content(string.Empty);
            }

            return View("Footnote", footnote);
        }
    }
}
