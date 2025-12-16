using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class AnyConditionTypeCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<Condition> anyConditionType = new();
            List<Condition> anyConditionTypeRenderConditions = schema.Pages
                .Where(page => page.Behaviours is not null)
                .SelectMany(page => page.Behaviours)
                .Where(behaviour => behaviour.Conditions is not null)
                .SelectMany(behaviour => behaviour.Conditions)
                .Where(condition => condition.ConditionType.Equals(ECondition.Any))
                .ToList();

            List<Condition> anyConditionTypeBehaviours = schema.Pages
                .Where(page => page.RenderConditions is not null)
                .SelectMany(page => page.RenderConditions)
                .Where(condition => condition.ConditionType.Equals(ECondition.Any))
                .ToList();

            anyConditionType.AddRange(anyConditionTypeRenderConditions);
            anyConditionType.AddRange(anyConditionTypeBehaviours);

            if (anyConditionType.Count.Equals(0))
                return result;

            anyConditionType.ForEach(condition =>
            {
                if (string.IsNullOrEmpty(condition.ComparisonValue))
                    result.AddFailureMessage("Any Condition Type Check, any condition type requires a comparison value.");
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}