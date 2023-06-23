using Data.Entities.Equipment;

namespace Data.Models.Newsletter;

/// <summary>
/// Viewmodel for Equipment.cshtml
/// </summary>
public class EquipmentModel
{
    public EquipmentModel(IEnumerable<Equipment> allEquipment, IEnumerable<Equipment> userEquipment)
    {
        AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
        UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
    }

    public IList<Equipment> AllEquipment { get; }
    public IList<Equipment> UserEquipment { get; }
}
