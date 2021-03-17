using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Page
{
    public interface IPageSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(Models.Page page);
        Task<IntegrityCheckResult> ValidateAsync(Models.Page page);
    }
}