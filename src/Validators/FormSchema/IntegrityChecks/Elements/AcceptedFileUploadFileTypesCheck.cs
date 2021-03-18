using form_builder.Enum;
using form_builder.Models.Elements;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AcceptedFileUploadFileTypesCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult integrityCheckResult = new();

            if (element.Type.Equals(EElementType.FileUpload) &&
                element.Properties.AllowedFileTypes != null)
            {
                element.Properties.AllowedFileTypes.ForEach(fileType =>
                {
                    if (!fileType.StartsWith("."))
                    {
                        integrityCheckResult.AddFailureMessage($"Accepted FileUpload File Types Check, Allowed file type in FileUpload element in form '{schema.FormName}', '{_.Properties.QuestionId}' must have a valid extension which begins with a '.', e.g. .png");
                    }
                });
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}