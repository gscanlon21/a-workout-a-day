using Data.Entities.Equipment;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for Equipment.cshtml
/// </summary>
public class EquipmentDto
{
    public EquipmentDto(IEnumerable<Equipment> allEquipment, IEnumerable<Equipment> userEquipment)
    {
        AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
        UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
    }

    public IList<Equipment> AllEquipment { get; }
    public IList<Equipment> UserEquipment { get; }
}
