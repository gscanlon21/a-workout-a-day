
using Core.Models.Equipment;
using Core.Models.Newsletter;

namespace Data.Query.Options;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;
    public int Id { get; }
    public int RefreshExercisesAfterXWeeks { get; }
    public Equipment Equipment { get; }
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }

    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;

    public UserOptions() { }

    public UserOptions(Entities.User.User user, Section? section)
    {
        NoUser = false;
        Id = user.Id;
        Equipment = user.Equipment;
        IsNewToFitness = user.IsNewToFitness;
        CreatedDate = user.CreatedDate;

        RefreshExercisesAfterXWeeks = section switch
        {
            Section.Functional => user.RefreshFunctionalEveryXWeeks,
            Section.Accessory => user.RefreshAccessoryEveryXWeeks,
            _ => 0
        };
    }
}
