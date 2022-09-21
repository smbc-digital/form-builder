using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public interface IBehaviourSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(List<Behaviour> behaviours);
        Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours);
    }
}