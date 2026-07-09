namespace form_builder.Helpers.RelativeDateHelper;

public interface IRelativeDateHelper
{
    bool HasValidDate(Element element, Dictionary<string, dynamic> viewModel);
    DateTime GetChosenDate(Element element, Dictionary<string, dynamic> viewModel);
    RelativeDate GetRelativeDate(string relativeDateString);
}