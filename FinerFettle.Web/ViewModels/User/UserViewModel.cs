using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            Progression = user.Progression;
            NeedsRest = user.NeedsRest;
            OverMinimumAge = user.OverMinimumAge;
            RestDays = user.RestDays;
        }

        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Range(0, 100)]
        public int? Progression { get; set; } = 50; // FIXME: Magic int is magic. Really the middle progression level.

        [Required]
        public bool NeedsRest { get; set; }

        [Required, MustBeTrue]
        public bool OverMinimumAge { get; set; }

        [Required]
        public RestDays RestDays { get; set; }

        public IList<Equipment>? Equipment { get; set; }

        public int[]? EquipmentBinder { get; set; }

        public RestDays[]? RestDaysBinder
        {
            get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
            set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
        }
    }
}
