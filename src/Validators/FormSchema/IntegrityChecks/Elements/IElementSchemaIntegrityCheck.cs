using form_builder.Models.Elements;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public interface IElementSchemaIntegrityCheck
    {
        IntegrityCheckResult Validate(IElement element);
        Task<IntegrityCheckResult> ValidateAsync(IElement element);
    }
}