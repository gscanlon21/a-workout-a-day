using FinerFettle.Web.Entities.Equipment;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class EquipmentViewModel
    {
        public EquipmentViewModel(IEnumerable<Equipment> allEquipment, IEnumerable<Equipment> userEquipment)
        {
            AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
            UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
        }

        public IList<Equipment> AllEquipment { get; }
        public IList<Equipment> UserEquipment { get; }
    }
}
