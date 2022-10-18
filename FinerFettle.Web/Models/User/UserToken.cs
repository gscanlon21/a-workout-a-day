using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User's progression level of an exercise.
    /// </summary>
    [Table("user_token"), Comment("Auth tokens for a user")]
    public class UserToken
    {
        public UserToken() { }

        public UserToken(int userId)
        {
            UserId = userId;

            Token = $"{Guid.NewGuid()}";
        }

        /// <summary>
        /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
        /// 
        /// This is kinda like a bearer token.
        /// </summary>
        [Required]
        public string Token { get; private set; } = null!;

        [Required]
        public int UserId { get; set; }
       
        public User User { get; set; } = null!;

        public DateOnly Expires { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
    }
}
