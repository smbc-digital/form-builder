using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class AnyConditionTypeCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var anyConditionType = new List<Condition>();
            List<Condition> anyConditionTypeRenderConditions = schema.Pages.Where(page => page.Behaviours != null)
                .SelectMany(page => page.Behaviours)
                .Where(behaviour => behaviour.Conditions != null)
                .SelectMany(behaviour => behaviour.Conditions)
                .Where(condition => condition.ConditionType == ECondition.Any)
                .ToList();

            List<Condition> anyConditionTypeBehaviours = schema.Pages.Where(page => page.RenderConditions != null)
                .SelectMany(page => page.RenderConditions)
                .Where(condition => condition.ConditionType == ECondition.Any)
                .ToList();

            anyConditionType.AddRange(anyConditionTypeRenderConditions);
            anyConditionType.AddRange(anyConditionTypeBehaviours);

            if (anyConditionType.Any())
            {
                anyConditionType.ForEach(condition =>
                {
                    if (string.IsNullOrEmpty(condition.ComparisonValue))
                        integrityCheckResult.AddFailureMessage($"Any Condition Type Check, any condition type requires a comparison value in form {schema.FormName}");
                });
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}