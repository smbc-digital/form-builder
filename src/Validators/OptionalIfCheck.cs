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
            
            List<IElement> elementsWithOptionalIfQuestionId = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId))
                .ToList();

            List<IElement> elementsWithOptionalIfComparisonValue = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIf.ComparisonValue))
                .ToList();

            if (elementsWithOptionalIfQuestionId.Count.Equals(0))
              return result;

            //TODOjordan after lunch - whys it null, why wont it work, maybe its the validators, unit tests, stu
            // foreach (var page in schema.Pages)
            // {
            //   foreach (var element in page.ValidatableElements)
            //   {
            //       questionIds.Add(element.Properties.QuestionId);

            //       if (!string.IsNullOrEmpty(element.Properties.OptionalIfValue) && !string.IsNullOrEmpty(element.Properties.OptionalIfNotValue))
            //           result.AddFailureMessage(
            //               $"The provided json has an element with both an OptionalIfValue and OptionalIfNotValue' QuestionId: '{element.Properties.QuestionId}'");

            //       if (element.Properties.Elements is not null && element.Properties.Elements.Count > 0)
            //       {
            //           foreach (var nestedElement in element.Properties.Elements)                        
            //               questionIds.Add(nestedElement.Properties.QuestionId);                        
            //       }
            //   }
            // }

            // foreach (var element in elementsWithOptionalIfQuestionId)
            // {
            //   if (!questionIds.Contains(element.Properties.OptionalIfQuestionId))
            //       result.AddFailureMessage(
            //              $"The provided json does not contain an OptionalIfQuestionId that matches to a QuestionId' QuestionId: '{element.Properties.QuestionId}'");
            // }

            // foreach (var element in elementsWithOptionalIfValue)
            // {
            //   if (string.IsNullOrEmpty(element.Properties.OptionalIfQuestionId))
            //       result.AddFailureMessage(
            //              $"The provided json has an OptionalIfValue with no OptionalIfQuestionId to compare it to: QuestionID: '{element.Properties.QuestionId}'");
            // }

            // foreach (var element in elementsWithOptionalIfNotValue)
            // {
            //   if (string.IsNullOrEmpty(element.Properties.OptionalIfQuestionId))
            //       result.AddFailureMessage(
            //              $"The provided json has an OptionalIfNotValue with no OptionalIfQuestionId to compare it to: QuestionID: '{element.Properties.QuestionId}'");
            // }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}