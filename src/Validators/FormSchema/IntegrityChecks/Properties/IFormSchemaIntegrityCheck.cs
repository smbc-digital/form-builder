using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Properties
{
    public interface IFormSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(FormSchema schema);
        Task<IntegrityCheckResult> ValidateAsync(FormSchema schema);
    }
}