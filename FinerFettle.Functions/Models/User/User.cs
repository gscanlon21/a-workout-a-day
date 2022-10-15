using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public int Id { get; init; }

        public string Email { get; set; } = null!;

        public bool AcceptedTerms { get; set; }

        public string? DisabledReason { get; set; } = null;

        public bool Disabled => DisabledReason != null;

        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public DateOnly? LastActive { get; set; } = null;

        [InverseProperty(nameof(Newsletter.Newsletter.User))]
        public virtual ICollection<Newsletter.Newsletter> Newsletters { get; set; } = default!;
    }
}
