namespace FinerFettle.Web.ViewModels.User
{
    /// <summary>
    /// A plain & simple message renderer.
    /// </summary>
    public class StatusMessageViewModel
    {
        public StatusMessageViewModel(string message)
        {
            Message = message;
        }

        public string Message { get; init; }
        public int? AutoCloseInXSeconds { get; init; } = 9;
    }
}
