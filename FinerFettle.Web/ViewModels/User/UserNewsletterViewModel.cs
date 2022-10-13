using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.User
{
    /// <summary>
    /// For the newsletter
    /// </summary>
    public class UserNewsletterViewModel
    {
        public UserNewsletterViewModel(Models.User.User user)
        {
            Id = user.Id;
            Email = user.Email;
            AverageProgression = user.AverageProgression;
            RecoveryMuscle = user.RecoveryMuscle;
            RestDays = user.RestDays;
            UserEquipments = user.UserEquipments;
            StrengtheningPreference = user.StrengtheningPreference;
            PrefersWeights = user.PrefersWeights;
            UserExercises = user.UserExercises;
            SportsFocus = user.SportsFocus;
            LastActive = user.LastActive;
        }

        public MuscleGroups RecoveryMuscle { get; }

        public SportsFocus SportsFocus { get; set; }

        public int Id { get; }

        public string Email { get; }

        public double AverageProgression { get; }

        public ICollection<UserEquipment> UserEquipments { get; set; } = new List<UserEquipment>();

        public RestDays RestDays { get; set; } = RestDays.None;

        public DateOnly LastActive { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public bool IsAlmostInactive => LastActive <= DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-11);

        public StrengtheningPreference StrengtheningPreference { get; set; } = StrengtheningPreference.Obtain;

        public bool PrefersWeights { get; set; }

        public ICollection<UserExercise> UserExercises { get; set; }
    }
}
