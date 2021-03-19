using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class DocumentDownloadCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (!schema.DocumentDownload)
                return result;

            if (schema.DocumentType.Count > 0 && 
                schema.DocumentType.Any(documentType => documentType.Equals(EDocumentType.Unknown)))
                    result.AddFailureMessage($"Document Download Check, Unknown document download type configured.");

            if (schema.DocumentType.Count == 0)
                result.AddFailureMessage($"Document Download Check, No document download type configured.");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
