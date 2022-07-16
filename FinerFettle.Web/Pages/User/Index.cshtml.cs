using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models;

namespace FinerFettle.Web.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly FinerFettle.Web.Data.NewsletterContext _context;

        public IndexModel(FinerFettle.Web.Data.NewsletterContext context)
        {
            _context = context;
        }

        public IList<Models.User> User { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Users != null)
            {
                User = await _context.Users.ToListAsync();
            }
        }
    }
}
