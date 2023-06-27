namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Equipment.cshtml
/// </summary>
public class EquipmentViewModel
{
    public IList<Equipment.EquipmentViewModel> AllEquipment { get; set; } = null!;
    public IList<Equipment.EquipmentViewModel> UserEquipment { get; set; } = null!;
}
