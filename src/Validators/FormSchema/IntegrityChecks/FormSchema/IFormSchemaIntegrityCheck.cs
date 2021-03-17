using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.FormSchema
{
    public interface IFormSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(Models.FormSchema schema);
        Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema);
    }
}