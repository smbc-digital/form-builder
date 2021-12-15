using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AcceptedFileUploadFileTypesCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Type.Equals(EElementType.FileUpload) && element.Properties.AllowedFileTypes is not null)
            {
                element.Properties.AllowedFileTypes.ForEach(fileType =>
                {
                    if (!fileType.StartsWith("."))
                    {
                        result.AddFailureMessage(
                            $"Accepted FileUpload File Types Check, " +
                            $"Allowed file type in FileUpload element, " +
                            $"'{element.Properties.QuestionId}' must have a valid extension which begins with a '.', e.g. .png");
                    }
                });
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}