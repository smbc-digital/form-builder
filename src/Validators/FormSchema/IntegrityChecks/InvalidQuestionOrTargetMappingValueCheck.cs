using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class InvalidQuestionOrTargetMappingValueCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            
            List<string> questionIds = schema.Pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Select(_ => string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping)
                .ToList();
            
            questionIds.ForEach(questionId =>
            {
                var regex = new Regex(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);

                if ((!regex.IsMatch(questionId.ToString()))
                    || questionId.ToString().EndsWith(".") 
                    || questionId.ToString().StartsWith("."))
                {
                    integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' contains invalid QuestionIDs or TargetMapping, '{questionId}' contains invalid characters");   
                }
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}