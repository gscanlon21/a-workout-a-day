using Core.Models.Exercise;
using Data.Entities.Users;

namespace Data.Query.Options.Users;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;

    public UserOptions() { }

    public UserOptions(User user)
    {
        Id = user.Id;
        NoUser = false;
        Equipment = user.Equipment;
        Intensity = user.Intensity;
        CreatedDate = user.CreatedDate;
        IsNewToFitness = user.IsNewToFitness;
    }

    public int Id { get; }
    public Equipment Equipment { get; }
    public Intensity Intensity { get; }
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }

    public bool NeedsDeload { get; set; } = false;
    public bool IgnoreIgnored { get; set; } = false;
    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;

    public bool HasData() => true;
}
