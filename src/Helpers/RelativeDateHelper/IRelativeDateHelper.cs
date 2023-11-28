using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Helpers.RelativeDateHelper
{
    public interface IRelativeDateHelper
    {
        bool HasValidDate(Element element, Dictionary<string, dynamic> viewModel);
        DateTime GetChosenDate(Element element, Dictionary<string, dynamic> viewModel);
        RelativeDate GetRelativeDate(string relativeDateString);
    }
}