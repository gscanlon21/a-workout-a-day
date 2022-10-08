using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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

        [Route("")]
        public IActionResult Index(bool wasUnsubscribed = false)
        {
            return View("Create", new UserViewModel() { 
                WasUnsubscribed = wasUnsubscribed 
            });
        }

        [AllowAnonymous, Route("user/validation/email")]
        public JsonResult IsUserAvailable(string email)
        {
            return Json(_context.Users.FirstOrDefault(u => u.Email == email) == null);
        }

        [Route("newsletter/signup"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,AcceptedTerms,IExist")] UserViewModel viewModel)
        {
            if (ModelState.IsValid && viewModel.IExist)
            {
                // User
                var newUser = new User()
                {
                    Email = viewModel.Email,
                    AcceptedTerms = viewModel.AcceptedTerms
                };

                _context.Add(newUser);
                try
                {
                    await _context.SaveChangesAsync();
                } 
                catch (DbUpdateException e) when (e.InnerException != null && e.InnerException.Message.Contains("duplicate key"))
                {
                    return RedirectToAction(nameof(Index), Name);
                }

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

                viewModel.WasSubscribed = true;
                return View("Create", viewModel);
            }

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
                .Include(u => u.ExerciseProgressions)
                .FirstOrDefaultAsync(m => m.Email == email);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel(user)
            {
                EquipmentBinder = user.EquipmentUsers.Select(e => e.EquipmentId).ToArray(),
                IgnoredExerciseBinder = user.ExerciseProgressions?.Where(ep => ep.Ignore).Select(e => e.ExerciseId).ToArray(),
                Equipment = await _context.Equipment.ToListAsync(),
                IgnoredExercises = await _context.Exercises.Where(e => user.ExerciseProgressions != null && user.ExerciseProgressions.Select(ep => ep.ExerciseId).Contains(e.Id)).ToListAsync(),
            };

            return View(viewModel);
        }

        [Route("user/edit/{email}"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string email, [Bind("Id,Email,RecoveryMuscle,SportsFocus,PrefersWeights,EmailVerbosity,EquipmentBinder,IgnoredExerciseBinder,RestDaysBinder,AcceptedTerms,StrengtheningPreference,Disabled")] UserViewModel viewModel)
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
                        .Include(u => u.ExerciseProgressions)
                        .FirstAsync(u => u.Id == viewModel.Id);

                    var oldUserProgressions = await _context.UserProgressions
                        .Where(p => p.UserId == viewModel.Id)
                        .Where(p => viewModel.IgnoredExerciseBinder != null && !viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
                        .ToListAsync();
                    var newUserProgressions = await _context.UserProgressions
                        .Where(p => p.UserId == viewModel.Id)
                        .Where(p => viewModel.IgnoredExerciseBinder != null && viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
                        .ToListAsync();
                    foreach (var oldUserProgression in oldUserProgressions)
                    {
                        oldUserProgression.Ignore = false;
                    }
                    foreach (var newUserProgression in newUserProgressions)
                    {
                        newUserProgression.Ignore = true;
                    }
                    _context.Set<ExerciseUserProgression>().UpdateRange(oldUserProgressions);
                    _context.Set<ExerciseUserProgression>().UpdateRange(newUserProgressions);

                    if (viewModel.RecoveryMuscle != Models.Exercise.MuscleGroups.None)
                    {
                        var progressions = _context.UserProgressions
                            .Where(up => up.UserId == viewModel.Id)
                            .Where(up => 
                                up.Exercise.PrimaryMuscles.HasFlag(viewModel.RecoveryMuscle)
                                || up.Exercise.SecondaryMuscles.HasFlag(viewModel.RecoveryMuscle)
                            );
                        foreach (var progression in progressions)
                        {
                            progression.Progression = 5;
                        }
                        _context.Set<ExerciseUserProgression>().UpdateRange(progressions);
                    }

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

                    oldUser.AcceptedTerms = viewModel.AcceptedTerms;
                    oldUser.EmailVerbosity = viewModel.EmailVerbosity;
                    oldUser.PrefersWeights = viewModel.PrefersWeights;
                    oldUser.RecoveryMuscle = viewModel.RecoveryMuscle;
                    oldUser.SportsFocus = viewModel.SportsFocus;
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

                return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. Changes will be reflected in the next email.") { 
                    AutoCloseInXSeconds = null 
                });
            }

            viewModel.Equipment = await _context.Equipment.ToListAsync();
            return View(viewModel);
        }

        [Route("user/{email}/fallback")]
        public async Task<IActionResult> ThatWorkoutWasTough(string email, int exerciseId, bool demo = false)
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

            return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%.")
            {
                Demo = demo
            });
        }

        [Route("user/{email}/ignore")]
        public async Task<IActionResult> IgnoreExercise(string email, int exerciseId)
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

            userProgression.Ignore = true;
            _context.Update(userProgression);
            await _context.SaveChangesAsync();

            return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved."));
        }

        [Route("user/{email}/advance")]
        public async Task<IActionResult> ThatWorkoutWasEasy(string email, int exerciseId, bool demo = false)
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

            return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%.")
            {
                Demo = demo
            });
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
                _context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserController.Index), new { WasUnsubscribed = true });
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
