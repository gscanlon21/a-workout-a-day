using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class EquipmentViewModel
    {
        public EquipmentViewModel(IEnumerable<Equipment> allEquipment, IEnumerable<Equipment> userEquipment)
        {
            AllEquipment = allEquipment.OrderBy(e => e.Name).ToList();
            UserEquipment = userEquipment.OrderBy(e => e.Name).ToList();
        }

        public IList<Equipment> AllEquipment { get; init; }
        public IList<Equipment> UserEquipment { get; init; }
    }
}
