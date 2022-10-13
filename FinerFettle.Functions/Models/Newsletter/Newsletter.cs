using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Functions.Models.Newsletter
{
    /// <summary>
    /// A day's workout routine.
    /// </summary>
    [Table("newsletter")]
    public class Newsletter
    {
        public int Id { get; init; }

        /// <summary>
        /// The date the newsletter was sent out on
        /// </summary>
        public DateOnly Date { get; init; }

        [InverseProperty(nameof(Models.User.User.Newsletters))]
        public User.User? User { get; set; }
    }
}
