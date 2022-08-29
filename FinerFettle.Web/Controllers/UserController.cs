using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using Microsoft.AspNetCore.Mvc.Rendering;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;

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

            var user = await _context.Users
                .Include(u => u.EquipmentUsers)
                .ThenInclude(u => u.Equipment)
                .FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel(user)
            {
                Equipment = user.EquipmentUsers.Select(e => e.Equipment).ToList()
            };

            return View(nameof(Details), viewModel);
        }

        [Route("user/{email}/fallback")]
        public async Task<IActionResult> ThatWorkoutWasTough(string email, int exerciseId)
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

            var userProgression = await _context.UserProgressions
                .Include(p => p.Exercise)
                .FirstAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

            userProgression.Progression -= 5;

            var validationContext = new ValidationContext(userProgression)
            {
                MemberName = nameof(userProgression.Progression)
            };
            if (Validator.TryValidateProperty(userProgression.Progression, validationContext, null))
            {
                _context.Update(userProgression);
                await _context.SaveChangesAsync();
            };

            return View("StatusMessage", $"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%");
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
        public async Task<IActionResult> ThatWorkoutWasEasy(string email, int exerciseId)
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

            var userProgression = await _context.UserProgressions
                .Include(p => p.Exercise)
                .FirstAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

            userProgression.Progression += 5;

            var validationContext = new ValidationContext(userProgression)
            {
                MemberName = nameof(userProgression.Progression)
            };
            if (Validator.TryValidateProperty(userProgression.Progression, validationContext, null))
            {
                _context.Update(userProgression);
                await _context.SaveChangesAsync();
            };

            return View("StatusMessage", $"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%");
        }

        [Route("user/create")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserViewModel()
            {
                Equipment = await _context.Equipment.ToListAsync()
            };

            return View(viewModel);
        }

        [Route("user/create"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,EquipmentBinder,EmailVerbosity,RestDaysBinder,OverMinimumAge,StrengtheningPreference,Disabled")] UserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // User
                var newUser = new User()
                {
                    Email = viewModel.Email,
                    OverMinimumAge = viewModel.OverMinimumAge,
                    NeedsRest = viewModel.NeedsRest,
                    EmailVerbosity = viewModel.EmailVerbosity,
                    RestDays = viewModel.RestDays,
                    StrengtheningPreference = viewModel.StrengtheningPreference,
                    Disabled = viewModel.Disabled
                };

                _context.Add(newUser);
                await _context.SaveChangesAsync();

                // User's Equipment
                var newEquipment = await _context.Equipment.Where(e =>
                    viewModel.EquipmentBinder != null && viewModel.EquipmentBinder.Contains(e.Id)
                ).ToListAsync();

                _context.TryUpdateManyToMany(Enumerable.Empty<EquipmentUser>(), newEquipment.Select(e =>
                    new EquipmentUser()
                    {
                        EquipmentId = e.Id,
                        UserId = newUser.Id
                    }),
                    x => x.EquipmentId
                );
                await _context.SaveChangesAsync();
 
                return RedirectToAction(nameof(Details), UserController.Name, new { viewModel.Email });
            }
            
            viewModel.Equipment = await _context.Equipment.ToListAsync();
            return View(viewModel);
        }

        [Route("user/edit/{email}")]
        public async Task<IActionResult> Edit(string? email)
        {
            if (email == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.EquipmentUsers)
                .FirstOrDefaultAsync(m => m.Email == email);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel(user)
            {
                EquipmentBinder = user.EquipmentUsers.Select(e => e.EquipmentId).ToArray(),
                Equipment = await _context.Equipment.ToListAsync()
            };

            return View(viewModel);
        }

        [Route("user/edit/{email}"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string email, [Bind("Id,Email,EmailVerbosity,EquipmentBinder,RestDaysBinder,OverMinimumAge,StrengtheningPreference,Disabled")] UserViewModel viewModel)
        {
            if (email != viewModel.Email)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldUser = await _context.Users
                        .Include(u => u.EquipmentUsers)
                        .FirstAsync(u => u.Id == viewModel.Id);

                    var newEquipment = await _context.Equipment.Where(e =>
                        viewModel.EquipmentBinder != null && viewModel.EquipmentBinder.Contains(e.Id)
                    ).ToListAsync();

                    _context.TryUpdateManyToMany(oldUser.EquipmentUsers, newEquipment.Select(e =>
                        new EquipmentUser() 
                        {
                            EquipmentId = e.Id,
                            UserId = viewModel.Id
                        }), 
                        x => x.EquipmentId
                    );

                    oldUser.OverMinimumAge = viewModel.OverMinimumAge;
                    oldUser.NeedsRest = viewModel.NeedsRest;
                    oldUser.EmailVerbosity = viewModel.EmailVerbosity;
                    oldUser.RestDays = viewModel.RestDays;
                    oldUser.StrengtheningPreference = viewModel.StrengtheningPreference;
                    oldUser.Disabled = viewModel.Disabled;

                    _context.Update(oldUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(viewModel.Id))
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

            viewModel.Equipment = await _context.Equipment.ToListAsync();
            return View(viewModel);
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

            return View(new UserViewModel(user));
        }

        [Route("user/delete/{email}"), HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string email)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'CoreContext.Users' is null.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user != null)
            {
                _context.Newsletters.RemoveRange(await _context.Newsletters.Where(n => n.User == user).ToListAsync());
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage("/Index");
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
