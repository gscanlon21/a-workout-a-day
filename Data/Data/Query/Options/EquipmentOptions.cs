namespace Data.Data.Query.Options;

public class EquipmentOptions : IOptions
{
    public EquipmentOptions() { }

    public EquipmentOptions(IEnumerable<int>? equipmentIds)
    {
        EquipmentIds = equipmentIds;
    }

    public IEnumerable<int>? EquipmentIds { get; set; }
}
