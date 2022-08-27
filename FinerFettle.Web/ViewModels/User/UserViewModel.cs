using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.User
{
    public class UserViewModel
    {
        public UserViewModel() { }
        public UserViewModel(Models.User.User user) 
        {
            Id = user.Id;
            Email = user.Email;
            NeedsRest = user.NeedsRest;
            OverMinimumAge = user.OverMinimumAge;
            RestDays = user.RestDays;
            StrengtheningPreference = user.StrengtheningPreference;
            Disabled = user.Disabled;
        }

        public int Id { get; set; }

        [Required, RegularExpression(@".*@.*(?<!gmail\.com\s*)$", ErrorMessage = "Invalid email. We cannot currently send to gmail addresses.")]
        public string Email { get; set; } = null!;

        [Required]
        public bool NeedsRest { get; set; }

        [Required, MustBeTrue]
        public bool OverMinimumAge { get; set; }

        public bool Disabled { get; set; }

        [Required]
        public StrengtheningPreference StrengtheningPreference { get; set; }

        [Required]
        public RestDays RestDays { get; set; }

        public IList<Equipment> Equipment { get; set; } = new List<Equipment>();

        public int[]? EquipmentBinder { get; set; }

        public RestDays[]? RestDaysBinder
        {
            get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
            set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
        }
    }
}
