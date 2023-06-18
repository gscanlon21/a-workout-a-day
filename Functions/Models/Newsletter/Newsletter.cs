using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinerFettle.Functions.Models.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter")]
public class Newsletter
{
    public int Id { get; private init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    public DateOnly Date { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Models.User.User.Newsletters))]
    public virtual User.User User { get; private init; } = null!;
}
