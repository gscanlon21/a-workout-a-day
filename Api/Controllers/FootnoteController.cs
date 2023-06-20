
using Core.Models.Footnote;
using Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FootnoteController : ControllerBase
{
    private readonly CoreContext _context;

    public FootnoteController(CoreContext context)
    {
        _context = context;
    }

    [HttpGet(Name = "GetFootnotes")]
    public async Task<IList<Data.Entities.Footnote.Footnote>> GetFootnotes(int count = 1, FootnoteType ofType = FootnoteType.Bottom)
    {
        // Only show the types the user wants to see
        //ofType &= user.FootnoteType;

        var footnotes = await _context.Footnotes
            // Has any flag
            .Where(f => (f.Type & ofType) != 0)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return footnotes;

        //if (footnotes == null || !footnotes.Any())
        //{
        //    return Content(string.Empty);
        //}

        //return View("Footnote", new FootnoteViewModel()
        //{
        //    User = user,
        //    Footnotes = footnotes
        //});
    }
}
