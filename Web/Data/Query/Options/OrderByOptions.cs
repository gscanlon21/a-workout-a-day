namespace Web.Data.Query.Options;

public class OrderByOptions
{
    public OrderByOptions() { }

    public OrderByOptions(OrderBy orderBy)
    {
        OrderBy = orderBy;
    }

    public OrderBy OrderBy { get; set; } = OrderBy.None;
    public int SkipCount { get; set; } = 0;
}
