using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Models
{
    [Comment("User who signed up for the newsletter")]
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
