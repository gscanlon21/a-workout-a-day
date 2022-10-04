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
        }

        public MuscleGroups? RecoveryMuscle { get; }

        public int Id { get; }

        public string Email { get; }

        public double AverageProgression { get; }
    }
}
