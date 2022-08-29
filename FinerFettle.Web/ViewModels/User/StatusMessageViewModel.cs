namespace FinerFettle.Web.ViewModels.User
{
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
