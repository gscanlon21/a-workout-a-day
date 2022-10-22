using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class CheckViewModel
    {
        public IList<string> Missing100PProgressionRange { get; init; } = null!;
        public IList<string> MissingRepRange { get; init; } = null!;
        public IList<string> MissingProficiency { get; init; } = null!;
    }
}
