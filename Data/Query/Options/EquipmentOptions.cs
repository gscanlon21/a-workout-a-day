using Core.Models.Equipment;

namespace Data.Query.Options;

public class EquipmentOptions : IOptions
{
    public EquipmentOptions() { }

    public EquipmentOptions(Equipment? equipments)
    {
        Equipment = equipments;
    }

    public Equipment? Equipment { get; set; }
}
