using Lib.Dtos.Equipment;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Equipment.cshtml
/// </summary>
public class EquipmentViewModel
{
    public EquipmentViewModel(IEnumerable<EquipmentDto> allEquipment, IEnumerable<EquipmentDto> userEquipment)
    {
        AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
        UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
    }

    public IList<EquipmentDto> AllEquipment { get; }
    public IList<EquipmentDto> UserEquipment { get; }
}
