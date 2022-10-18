using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinerFettle.Functions.Models.User
{
    /// <summary>
    /// User's progression level of an exercise.
    /// </summary>
    [Table("user_token")]
    public class UserToken
    {
        /// <summary>
        /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
        /// 
        /// This is kinda like a bearer token.
        /// </summary>
        public string Token { get; private set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        /// <summary>
        /// Unsubscribe links need to work for at least 60 days per law
        /// </summary>
        public DateOnly Expires { get; set; }
    }
}
