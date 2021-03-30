using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public interface IFormSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(Models.FormSchema schema);
        Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema);
    }
}