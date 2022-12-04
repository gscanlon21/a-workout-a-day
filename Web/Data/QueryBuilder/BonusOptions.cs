using Web.Models.User;

namespace Web.Data.QueryBuilder;

public class BonusOptions
{
    public BonusOptions() { }

    public BonusOptions(Bonus? bonus)
    {
        Bonus = bonus;
    }

    public Bonus? Bonus { get; private set; } = null;

    public bool OnlyBonus { get; set; } = false;
}
