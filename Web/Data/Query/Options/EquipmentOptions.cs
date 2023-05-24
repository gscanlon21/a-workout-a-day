namespace Web.Data.Query.Options;

public class EquipmentOptions
{
    public EquipmentOptions() { }

    public EquipmentOptions(IEnumerable<int>? equipmentIds)
    {
        EquipmentIds = equipmentIds;
    }

    public IEnumerable<int>? EquipmentIds { get; set; }
}
