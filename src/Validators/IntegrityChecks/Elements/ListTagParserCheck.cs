using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements;

public class ListTagParserCheck : IElementSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(IElement element)
    {
        IntegrityCheckResult result = new();

        if (element.Properties is null || (string.IsNullOrEmpty(element.Properties.IAG) && string.IsNullOrEmpty(element.Properties.Hint) && !element.Properties.ListItems.Any()))
            return result;

        Regex regex = new(@"\{\{([UO]LIST)(?<!:):(?!:).*?\}\}");

        MatchCollection matchesInIag = regex.Matches(element.Properties.IAG ?? string.Empty);
        MatchCollection matchesInHint = regex.Matches(element.Properties.Hint ?? string.Empty);

        foreach (Match match in matchesInIag)
        {
            result.AddFailureMessage($"Element with ListTagParser '{match.Value}' requires a double : separator in the IAG property.");
        }
        foreach (Match match in matchesInHint)
        {
            result.AddFailureMessage($"Element with ListTagParser '{match.Value}' requires a double : separator in the Hint property.");
        }

        foreach (string listItem in element.Properties.ListItems)
        {
            MatchCollection matchesInListItem = regex.Matches(listItem);
            foreach (Match match in matchesInListItem)
            {
                result.AddFailureMessage($"Element with ListTagParser '{match.Value}' requires a double : separator in the ListItems property.");
            }
        }

        return result;
    }

    public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
}
