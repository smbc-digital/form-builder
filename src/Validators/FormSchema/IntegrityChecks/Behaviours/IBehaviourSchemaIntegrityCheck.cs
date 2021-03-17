using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public interface IBehaviourSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(Behaviour behaviour);
        Task<IntegrityCheckResult> ValidateAsync(Behaviour behaviour);
    }
}