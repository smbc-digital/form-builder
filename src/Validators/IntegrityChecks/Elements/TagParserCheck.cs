using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements;

public class TagParserCheck : IElementSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(IElement element)
    {
        IntegrityCheckResult result = new();

        if (element.Properties is null || (string.IsNullOrEmpty(element.Properties.Text) && string.IsNullOrEmpty(element.Properties.Hint)))
            return result;

        Regex regex = new(@"\{\{(EMPHASIS|STRONG|ITALIC|BOLD|IMAGE)(?<!:):(?!:).*?\}\}");
        MatchCollection matchesInText = regex.Matches(element.Properties.Text ?? string.Empty);
        MatchCollection matchesInHint = regex.Matches(element.Properties.Hint ?? string.Empty);

        foreach (Match match in matchesInText)
        {
            result.AddFailureMessage($"Element with TagParser '{match.Value}' requires a double : separator in the Text property.");
        }

        foreach (Match match in matchesInHint)
        {
            result.AddFailureMessage($"Element with TagParser '{match.Value}' requires a double : separator in the Hint property.");
        }

        return result;
    }
    public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
}
