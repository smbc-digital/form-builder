using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class OptionalIfCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();
            List<string> questionIds = new();

            List<IElement> elementsWithOptionalIfInJSON = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId) || !string.IsNullOrEmpty(element.Properties.OptionalIf.ComparisonValue) || !element.Properties.OptionalIf.ConditionType.Equals(Enum.ECondition.Undefined))
                .ToList();

            if (elementsWithOptionalIfInJSON.Count.Equals(0))
              return result;

            foreach (var page in schema.Pages)
            {
                foreach (var element in page.ValidatableElements)
                {
                    questionIds.Add(element.Properties.QuestionId);

                    if (element.Properties.Elements is not null && element.Properties.Elements.Count > 0)
                    {
                        foreach (var nestedElement in element.Properties.Elements)
                            questionIds.Add(nestedElement.Properties.QuestionId);
                    }
                }
            }

            foreach (var element in elementsWithOptionalIfInJSON)
            {
                if(string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId))
                    result.AddFailureMessage(
                           $"The provided json has an OptionalIf that does not contain a QuestionId. Error on: '{element.Properties.QuestionId}'");

                if (string.IsNullOrEmpty(element.Properties.OptionalIf.ComparisonValue))
                    result.AddFailureMessage(
                           $"The provided json has an OptionalIf that does not contain a ComparisonValue. Error on: '{element.Properties.QuestionId}'");

                if (element.Properties.OptionalIf.ConditionType.Equals(Enum.ECondition.Undefined))
                    result.AddFailureMessage(
                           $"The provided json contains an OptionalIf that does not contain a ConditionType. Error on: '{element.Properties.QuestionId}'");

                if (!string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId) && !questionIds.Contains(element.Properties.OptionalIf.QuestionId))
                    result.AddFailureMessage(
                           $"The provided json contains an OptionalIf that does not match to a QuestionId in the form. Error on: '{element.Properties.QuestionId}'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}