using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Components.Newsletter;

public class PreferencesViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Preferences";

    private readonly CoreContext _context;

    public PreferencesViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(UserNewsletterViewModel user)
    {
        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        return View("Preferences", new PreferencesViewModel(user)
        {
            AllEquipment = equipmentViewModel
        });
    }
}
