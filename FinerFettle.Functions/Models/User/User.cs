using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Functions.Models.User
{
    /// <summary>
    /// User who signed up for the newsletter.
    /// </summary>
    [Table("user")]
    [DebuggerDisplay("Email = {Email}, Disabled = {Disabled}")]
    public class User
    {
        public int Id { get; private init; }

        public string Email { get; private init; } = null!;

        public bool AcceptedTerms { get; private init; }

        public string? DisabledReason { get; set; } = null;

        public bool Disabled => DisabledReason != null;

        public DateOnly CreatedDate { get; private init; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public DateOnly? LastActive { get; private init; } = null;

        [InverseProperty(nameof(Newsletter.Newsletter.User))]
        public virtual ICollection<Newsletter.Newsletter> Newsletters { get; private init; } = null!;

        [InverseProperty(nameof(UserToken.User))]
        public virtual ICollection<UserToken> UserTokens { get; private init; } = null!;
    }
}
