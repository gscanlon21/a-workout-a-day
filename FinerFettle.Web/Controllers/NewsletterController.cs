using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Newsletter;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CoreContext _context;

        public NewsletterController(CoreContext context)
        {
            _context = context;
        }

        [Route("newsletter")]
        public async Task<IActionResult> Index()
        {
            var currentDate = DateTime.Today;
            return await Newsletter(currentDate.Year, currentDate.Month, currentDate.Day);
        }

        [Route("newsletter/{year}/{month}/{day}")]
        public async Task<IActionResult> Newsletter(int year, int month, int day)
        {
            var date = new DateOnly(year, month, day);
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (date > today)
            {
                return Problem("The workout routine for that day is undecided.");
            }

            var previousNewsletter = await _context.Newsletters.FirstOrDefaultAsync(m => m.Date == date);
            if (previousNewsletter != null)
            {
                return View(nameof(Newsletter), null);
            }

            if ((date < today && previousNewsletter == null) || _context.Exercises == null)
            {
                return NotFound();
            }

            // TODO: Pull a structured workout routine for each new day
            var exercises = (await _context.Exercises.ToListAsync()).Take(5).ToList();

            // TODO: Save the previous exercises/muscles worked so that a variety is sent out
            var newsletter = new Newsletter()
            {
                Date = date
            };

            //_context.Newsletters.Add(newsletter);
            //await _context.SaveChangesAsync();

            return View(nameof(Newsletter), exercises);
        }
    }
}
