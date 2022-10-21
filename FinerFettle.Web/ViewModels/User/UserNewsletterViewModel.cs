﻿using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.User
{
    /// <summary>
    /// For the newsletter
    /// </summary>
    public class UserNewsletterViewModel
    {
        public UserNewsletterViewModel(Models.User.User user, string token)
        {
            Id = user.Id;
            Email = user.Email;
            RecoveryMuscle = user.RecoveryMuscle;
            RestDays = user.RestDays;
            UserEquipments = user.UserEquipments;
            StrengtheningPreference = user.StrengtheningPreference;
            PrefersWeights = user.PrefersWeights;
            UserExercises = user.UserExercises;
            SportsFocus = user.SportsFocus;
            LastActive = user.LastActive;
            EmailVerbosity = user.EmailVerbosity;
            Token = token;
        }

        public int Id { get; }

        public string Email { get; }

        public string Token { get; }

        public bool PrefersWeights { get; set; }

        public ICollection<UserEquipment> UserEquipments { get; set; } = new List<UserEquipment>();

        public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId) ?? new List<int>();

        public RestDays RestDays { get; set; } = RestDays.None;

        public MuscleGroups RecoveryMuscle { get; }

        public SportsFocus SportsFocus { get; set; }

        public Verbosity EmailVerbosity { get; set; }

        public StrengtheningPreference StrengtheningPreference { get; set; } = StrengtheningPreference.Obtain;

        public ICollection<UserExercise> UserExercises { get; set; }

        public DateOnly? LastActive { get; set; } = null;

        public bool IsAlmostInactive => LastActive != null && LastActive < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-5);
    }
}
