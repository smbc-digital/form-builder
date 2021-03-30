using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class HasDuplicateQuestionIdsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();
            List<string> questionIds = new();

            foreach (var page in schema.Pages)
            {
                foreach (var element in page.ValidatableElements)
                {
                    questionIds.Add(element.Properties.QuestionId);
                }
            }

            HashSet<string> hashSet = new();
            if (questionIds.Any(id => !hashSet.Add(id)))
                result.AddFailureMessage($"The provided json has duplicate QuestionIDs");
            
            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}