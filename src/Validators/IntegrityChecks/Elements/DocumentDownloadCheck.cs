using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class DocumentDownloadCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.DocumentDownload))
                return result;

            if (element.Properties is null)
                return result;

            if (element.Properties.DocumentType.Equals(EDocumentType.Unknown))
                result.AddFailureMessage($"Document Download Check, '{element.Properties.QuestionId}' requires valid DocumentType");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}