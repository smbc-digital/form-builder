using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class DocumentDownloadCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (!schema.DocumentDownload)
                return IntegrityCheckResult.ValidResult;

            if (schema.DocumentType.Any() && schema.DocumentType.Any(_ => _ == EDocumentType.Unknown))
            {
                integrityCheckResult.AddFailureMessage($"Document Download Check, Unknown document download type configured for form '{ schema.FormName }'");
            }
            else
            {
                integrityCheckResult.AddFailureMessage($"Document Download Check, No document download type configured for form '{ schema.FormName }'");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}