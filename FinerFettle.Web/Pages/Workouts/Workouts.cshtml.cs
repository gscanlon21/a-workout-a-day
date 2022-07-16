using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models;

namespace FinerFettle.Web.Pages.Workouts
{
    public class WorkoutsModel : PageModel
    {
        private readonly FinerFettle.Web.Data.NewsletterContext _context;

        public WorkoutsModel(FinerFettle.Web.Data.NewsletterContext context)
        {
            _context = context;
        }

        public IList<Exercise> Workout { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Exercises != null)
            {
                Workout = await _context.Exercises.ToListAsync();
            }
        }
    }
}
