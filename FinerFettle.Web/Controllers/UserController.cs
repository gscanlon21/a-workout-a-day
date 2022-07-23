using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "User";

        public UserController(CoreContext context)
        {
            _context = context;
        }

        [Route("user/{email}")]
        public async Task<IActionResult> Details(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            return View(nameof(Details), user);
        }

        [Route("user/{email}/fallback")]
        public async Task<IActionResult> ThatWorkoutWasTough(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            user.Progression -= 10;
            user.NeedsRest = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return await Details(user.Email);
        }

        [Route("user/{email}/rest")]
        public async Task<IActionResult> INeedRest(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            user.NeedsRest = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return await Details(user.Email);
        }

        [Route("user/{email}/advance")]
        public async Task<IActionResult> ThatWorkoutWasEasy(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            user.Progression += 5;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return await Details(user.Email);
        }

        [Route("user/create")]
        public IActionResult Create()
        {
            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("user/create"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Progression,EquipmentBinder,RestDaysBinder")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), UserController.Name, new { user.Email });
            }

            return View(user);
        }

        [Route("user/edit/{email}")]
        public async Task<IActionResult> Edit(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("user/edit/{email}"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string email, [Bind("Id,Email,Progression,EquipmentBinder,RestDaysBinder")] User user)
        {
            if (email != user.Email)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Details), UserController.Name, new { email });
            }

            return View(user);
        }

        [Route("user/delete/{email}")]
        public async Task<IActionResult> Delete(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Route("user/delete/{email}"), HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string email)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'CoreContext.Users'  is null.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
