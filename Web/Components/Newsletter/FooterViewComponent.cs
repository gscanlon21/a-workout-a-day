﻿using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Components.Newsletter;

/// <summary>
/// Renders the user's preferences and links of a newsletter.
/// </summary>
public class FooterViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Footer";

    private readonly CoreContext _context;

    public FooterViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(UserNewsletterViewModel user)
    {
        var equipmentViewModel = new ViewModels.Newsletter.EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        return View("Footer", new FooterViewModel(user)
        {
            AllEquipment = equipmentViewModel
        });
    }
}
