using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public interface IFormSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(FormSchema schema);
        Task<IntegrityCheckResult> ValidateAsync(FormSchema schema);
    }
}