using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public interface IBehaviourSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(List<Behaviour> behaviours, string formName);
        Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours, string formName);
    }
}