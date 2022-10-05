using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using System.ComponentModel.DataAnnotations;

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
            EquipmentUsers = user.EquipmentUsers;
            StrengtheningPreference = user.StrengtheningPreference;
            PrefersWeights = user.PrefersWeights;
            ExerciseProgressions = user.ExerciseProgressions;
            SportsFocus = user.SportsFocus;
        }

        public MuscleGroups? RecoveryMuscle { get; }

        public SportsFocus SportsFocus { get; set; }

        public int Id { get; }

        public string Email { get; }

        public double AverageProgression { get; }

        public ICollection<EquipmentUser> EquipmentUsers { get; set; } = new List<EquipmentUser>();

        public RestDays RestDays { get; set; } = RestDays.None;

        public StrengtheningPreference StrengtheningPreference { get; set; } = StrengtheningPreference.Obtain;

        public bool PrefersWeights { get; set; }

        public ICollection<ExerciseUserProgression> ExerciseProgressions { get; set; }
    }
}
