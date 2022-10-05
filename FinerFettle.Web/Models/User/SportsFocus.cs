namespace FinerFettle.Web.Models.User
{
    [Flags]
    public enum SportsFocus
    {
        None = 0,
        Tennis = 1 << 0,
        Soccer = 1 << 1,
        Hockey = 1 << 2,
        Baseball = 1 << 3,
        Boxing = 1 << 4
    }
}
