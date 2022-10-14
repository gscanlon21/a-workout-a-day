using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.User
{
    /// <summary>
    /// For CRUD actions
    /// </summary>
    public class UserViewModel
    {
        public UserViewModel() { }

        public UserViewModel(Models.User.User user) 
        {
            Id = user.Id;
            Email = user.Email;
            AcceptedTerms = user.AcceptedTerms;
            RestDays = user.RestDays;
            StrengtheningPreference = user.StrengtheningPreference;
            Disabled = user.Disabled;
            DisabledReason = user.DisabledReason;
            EmailVerbosity = user.EmailVerbosity;
            PrefersWeights = user.PrefersWeights;
            RecoveryMuscle = user.RecoveryMuscle;
            SportsFocus = user.SportsFocus;
        }

        /// <summary>
        /// User jsut subscribed
        /// </summary>
        public bool WasSubscribed { get; set; }

        /// <summary>
        /// User just unsubscribed
        /// </summary>
        public bool WasUnsubscribed { get; set; }

        public int Id { get; set; }

        [Required, RegularExpression(@"\s*\S+@\S+\.\S+\s*", ErrorMessage = "Invalid email.")]
        [Remote(nameof(Controllers.UserController.IsUserAvailable), Controllers.UserController.Name, ErrorMessage = "Invalid email. Manage your preferences using the link in the newsletter.")]
        [DisplayName("Email")]
        public string Email { get; set; } = null!;

        [Required, MustBeTrue]
        public bool AcceptedTerms { get; set; }

        /// <summary>
        /// Anti-bot honeypot
        /// </summary>
        public bool IExist { get; set; }

        /// <summary>
        /// Pick weighted variations over calisthenics if available
        /// </summary>
        [Required, DisplayName("Prefer Weights")]
        public bool PrefersWeights { get; set; }

        /// <summary>
        /// Don't strengthen this muscle group, but do show recovery variations for exercises
        /// </summary>
        [DisplayName("Recovery Muscle")]
        public MuscleGroups RecoveryMuscle { get; set; }

        /// <summary>
        /// Include a section to boost a specific sports performance
        /// </summary>
        [DisplayName("Sports Focus")]
        public SportsFocus SportsFocus { get; set; }

        [DisplayName("Disabled Reason")]
        public string? DisabledReason { get; set; }

        [DisplayName("Disabled")]
        public bool Disabled { get; set; }

        [Required]
        [DisplayName("Strengthening Preference")]
        public StrengtheningPreference StrengtheningPreference { get; set; }

        [Required]
        [DisplayName("Email Verbosity")]
        public Verbosity EmailVerbosity { get; set; }

        [Required]
        [DisplayName("Rest Days")]
        public RestDays RestDays { get; set; }

        [DisplayName("Equipment")]
        public IList<Equipment> Equipment { get; set; } = new List<Equipment>();

        public int[]? EquipmentBinder { get; set; }

        [DisplayName("Ignored Exercises")]
        public IList<Exercise> IgnoredExercises { get; set; } = new List<Exercise>();

        public int[]? IgnoredExerciseBinder { get; set; }

        public RestDays[]? RestDaysBinder
        {
            get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
            set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
        }
    }
}
