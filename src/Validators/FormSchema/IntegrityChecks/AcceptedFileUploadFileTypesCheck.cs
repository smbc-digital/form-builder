using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks
{
    public class AcceptedFileUploadFileTypesCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            List<IElement> documentUploadElements = schema.Pages.Where(page => page.Elements != null)
                .SelectMany(page => page.ValidatableElements)
                .Where(element => element.Type == EElementType.FileUpload)
                .Where(element => element.Properties.AllowedFileTypes != null)
                .ToList();

            documentUploadElements.ForEach(_ =>
            {
                _.Properties.AllowedFileTypes.ForEach(fileType =>
                {
                    if (!fileType.StartsWith("."))
                        integrityCheckResult.AddFailureMessage($"Accepted FileUpload File Types Check, Allowed file type in FileUpload element in form '{schema.FormName}', '{_.Properties.QuestionId}' must have a valid extension which begins with a '.', e.g. .png");
                });
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}