using Lib.ViewModels.Equipment;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Equipment.cshtml
/// </summary>
public class EquipmentViewModel
{
    public EquipmentViewModel(IEnumerable<Equipment.EquipmentViewModel> allEquipment, IEnumerable<Equipment.EquipmentViewModel> userEquipment)
    {
        AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
        UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
    }

    public IList<Equipment.EquipmentViewModel> AllEquipment { get; }
    public IList<Equipment.EquipmentViewModel> UserEquipment { get; }
}
